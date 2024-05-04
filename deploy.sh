# Update submodules
git submodule update --init --recursive

# Build site
hugo

# Commit new version and push
cd public
git add .
git commit -m "Update blog"
git push