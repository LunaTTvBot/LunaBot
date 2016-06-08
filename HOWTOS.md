# HowTo's for Beginners
## Sync your fork
##### 1. Add upstream Remote
`git remote add upstream https://github.com/LunaTTvBot/iBot.git`
##### 2. Fetch upstream
`git fetch upstream`
##### 3. Make sure you're in develop branch
`git checkout develop`
##### 4. Merge
`git merge upstream/develop`
##### 5. Push to your Fork
`git push origin`
##### 6. Update your Feature Branch
`git checkout YOUR_AWESOME_BRANCH`
##### 7. Make a rebase
`git rebase develop`  
> If you're not this familiar with rebasing you can use merge  
> But if you can __USE__ rebase  
> `git merge develop`
> If you have any Merge Conflicts you can get help [here](https://help.github.com/articles/resolving-a-merge-conflict-from-the-command-line/)

##### 8. Push to your Fork
`git push origin`
