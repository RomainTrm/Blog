# Personal blog

This repository regroup all my blog post in the markdown format.  

## How to comment

If you wish to comment on a post, please send me a pull request. All posts are available in the `content\posts` folder.  
Just add your comment at the end of the post, here's a format suggestion (I may adjust formatting before accepting the pull request if too messy):

```markdown
> [Romain](https://romaintrm.github.io/) - 2025/03/04 14:23  
>
> Your point is interesting, though I think [...]
```

Looking:  

> [Romain](https://romaintrm.github.io/) - 2025/03/04 14:23  
>
> Your point is interesting, though I think [...]

Note, a link to one of your social media isn't mandatory.

## Credits

- [Hugo](https://gohugo.io/getting-started/quick-start/)
- [Hugo Rocinante theme](https://themes.gohugo.io/themes/hugo-rocinante/)  

## Personal reminders

Fetch and pull themes:

```bash
git fetch
git submodule update --init --recursive
```

or  

```bash
git submodule sync --recursive
```

Add new post:

```bash
hugo new content posts/<post-name>/index.md
```

Builder server with drats for preview:

```bash
hugo server -D -F
```