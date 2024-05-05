# Update submodules (Troubles with this command)
# git submodule update --init --recursive

# Alternate solution
# Update public
cd public
git add .
git reset --hard
git pull --rebase
cd ..

# Build site
hugo

# Commit new version and push
cd public
git add .
git commit -m "Update blog"
git push