# Personal blog

Migration of my old technical blog

## Build reminders

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
hugo new content posts/<post-name>.md
```

Builder server with drats for preview:

```bash
hugo server -D
```

## Credits

- [Hugo](https://gohugo.io/getting-started/quick-start/)
- [Hugo Rocinante theme](https://themes.gohugo.io/themes/hugo-rocinante/)  
