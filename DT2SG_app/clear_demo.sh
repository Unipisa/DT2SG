rm -i -f -R demo
cp -r demo_clear demo 
cd demo
git init 
git add .
git commit -m "repo created"
echo "empty file test" >> new_file.txt
git add .
git commit -m "new file created"
echo "empty file test" >> new_file_again.txt
git add .
git commit -m "new file created again"