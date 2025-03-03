---
title: "ZERO DOWNTIME DATABASE MIGRATION"
date: 2024-12-04T09:13:43+01:00
tags: [post, en]
draft: false
aliases: ["/posts/2024-12-04/"]
---

Nowadays, most of the services we're using are online and available 24/7. If, like me, you're working on a company that provide this kind of service, you're probably aiming for such availability. As I've already [highlighted it](/posts/2022-09-01/), it has a huge influence on how you should code and deploy your software. Indeed, to maximize availability, you're probably aiming for a zero downtime deployment.  

Zero downtime deployment includes several topics. Today I want to focus on how to achieve a database migration without service interruption.

## REQUIREMENTS

First, be aware that database schema migration isn't the first topic you'll have to address to achieve zero downtime migrations.  

As a requirement, you need to step up a deployment pipeline with at least a couple instances of your service and a proxy/load-balancer to route traffic on them. As we'll need to do several deployments, for your own safety and sanity it's better to fully automate this process.  

```goat
             .-------.                                                             
             | Proxy |
             '-+---+-'
              /     \
             v       v
.-------------.     .-------------.
| Instance 1  |     | Instance 2  |
| Version 1.0 |     | Version 1.0 |
'-----------*-'     '-*-----------'
             \       /
              v     v
            .----------.
            | Database |
            '----------'
```

Clients are calling our service through the proxy, it's in charge to dispatch requests to our instances.  
When deploying a new version, you need to proceed a rolling update. The first step is to update the proxy to call only one instance, then you can update the isolated one.  

```goat
             .-------.                                                             
             | Proxy |
             '-+---+-'   Update
              /            |
             v             v
.-------------.     .-------------.
| Instance 1  |     | Instance 2  |
| Version 1.0 |     | Version 2.0 |
'-----------*-'     '-*-----------'
             \       /
              v     v
            .----------.
            | Database |
            '----------'
```

Once updated, change the proxy to route all traffic to the updated instance. Now you can update the old one.  

```goat
             .-------.                                                             
             | Proxy |
   Update    '-+---+-'
      |             \
      v              v
.-------------.     .-------------.
| Instance 1  |     | Instance 2  |
| Version 2.0 |     | Version 2.0 |
'-----------*-'     '-*-----------'
             \       /
              v     v
            .----------.
            | Database |
            '----------'
```

Then you can resume to normal operation by dispatching requests to both instances.  

```goat
             .-------.                                                             
             | Proxy |
             '-+---+-'
              /     \
             v       v
.-------------.     .-------------.
| Instance 1  |     | Instance 2  |
| Version 2.0 |     | Version 2.0 |
'-----------*-'     '-*-----------'
             \       /
              v     v
            .----------.
            | Database |
            '----------'
```

Now we're able to migrate our database without any service interruption.

## MIGRATION

Let's details steps for a zero downtime migration. The core idea is to always have a database schema supported by versions _N_ and _N+1_ of your software. Furthermore, it allows you to rollback the latest software deployment at any time if you detect a blocking issue.

Note: it's important to respect these steps, you cannot skip or reorder any of them.  

### STEP 1: FIRST SCHEMA MIGRATION

To begin, we have to deploy a first schema migration, but there is some constraints. For now, we're aiming for a schema that supports both old and new data format. We're not migrating the data yet.  

It's already a tricky step because we don't want to accidentally lock a table for a long period of time. Long locks have an impact on our service: it can be perceived as slower by the users and some queries may timeout. This requires some knowledge about database behavior to avoid these.  

We have to adopt different strategies depending of what type of migration we're targeting:

| Target | Migration to apply |
| - | - |
| Add a new column, rename/edit an existing column | Create a new column without `NOT NULL` constraint, it's probably better to avoid `DEFAULT VALUE` too. |
| Remove a column | Remove existing `NOT NULL` constraint. |

### STEP 2: FIRST SOFTWARE MIGRATION

Time for the second deployment, now it's a software update. In this version, the shipped code must:  

| Target | Code behavior |
| - | - |
| Add a new column | Write but do not read the new column. |
| Rename/edit an existing column | Write but do not read the new column. Keep writing and reading the old one. |
| Remove a column | Keep writing in the column but do not read it anymore. |

By doing so, we're maintaining the old schema and starting to fill the new one without exposing ourselves to the unpopulated data.  

If you're using an [ORM](https://en.wikipedia.org/wiki/Object%E2%80%93relational_mapping) like Entity Framework to generate your requests, be very careful with the columns it accesses in its queries.

### STEP 3: MIGRATE DATA

If we're only removing a column, we can skip this step. Otherwise, it's time to populate the new column.  

To do so, we have to write and apply a script that fills unpopulated rows. As in _step 1_, we must avoid locking tables. The best strategy is probably to update a small number of lines in dedicated transactions and loop until the table is fully filled.  

> Tip: once this is done, you can wait a bit to make sure you haven't forgotten to update a write request. In such case, new rows with empty values should appear.  

### STEP 4: SECOND SOFTWARE MIGRATION

Now we can migrate our software to our final target.  

| Target | Code behavior |
| - | - |
| Add a new column | Write and read the new column. |
| Rename/edit an existing column | Write and read the new column. Note: You can choose not to write anymore in the old column, but it breaks the backward compatibility. You can delay this "cleanup" to a future version. |
| Remove a column | Do not read neither write the old column. |

### STEP 5: SECOND SCHEMA MIGRATION

And finally, we have to cleanup the database.  

| Target | Code behavior |
| - | - |
| Add a new column | Update constraints to add `NOT NULL` and `DEFAULT VALUE` (if needed). |
| Rename/edit an existing column | Update constraints to add `NOT NULL` and `DEFAULT VALUE` (if needed). If you chose not to write in the old column, you can drop it. |
| Remove a column | Do the column. |

In this step, we're not worried by locks because rows are filled: the application of these constraints does not trigger update of the rows.

## CONCLUSION

As we've seen, this is not a straightforward procedure. It requires more work, the ability to manage intermediate versions and some continuous deployment culture from the team. However, it gives you a lot of freedom for deployment, you can ship a new version at any time. It can be beneficial if your clients are other businesses with high availability constraints, you will not be stuck maintaining several versions of your software because they don't want to stop their service for an update.  

This migration process isn't always necessary (unless if you want to ensure backward compatibility). Well-isolated asynchronous processes can be updated with a service interruption, they will catch up when the process is resumed.

If you have full control of your deployment pipeline and you can ensure you are fully updating your database before starting to deploy the new software version, then this process can be optimized:

- merge _step 1_ with _step 2_
- merge _step 3_ with _step 4_, but as I mentioned it, waiting after _step 3_ can increase your confidence in your migration  

## MORE ON THIS TOPIC

I recommend for my French readers this [talk](https://youtu.be/pIkA-aPtkNs) that is way more detailed than this blog post.  

---

## COMMENTS

<!--Add your comment here-->

Wish to comment? Please, add your comment by [sending me a pull request](https://github.com/RomainTrm/Blog?tab=readme-ov-file#how-to-comment).
