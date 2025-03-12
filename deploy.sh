# Update submodules (Troubles with this command)
# git submodule update --init --recursive

# Alternate solution
# Update public
cd public
git add .
git reset --hard
git pull --rebase
cd ..

# Update theme
cd themes/rocinante
git add .
git reset --hard
git checkout custom
git pull --rebase
cd ../..

# Build site
hugo

# Commit new version and push
cd public
git add .
git commit -m "Update blog"
git push

# Don't close window at the end
echo "Deployment done"
read