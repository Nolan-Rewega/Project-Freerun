# Basic Github Tutorial 

In order to get the current state of the repository initially, clone the repository to a folder you want the project to exist in. 
The simplest way to do this is as follows: Open a command prompt in the folder you want the repository, or inside the folder on windows 
shift + right click -> open powershell and enter the following 

``` git clone https://github.com/Nolan-Rewega/Project-Freerun ```

The repository should now be in the folder you want. To create your own development branch enter the following: 

``` git branch mybranch ```

You have now created a local branch. If you want to develop on this branch, you need to check it out:

``` git checkout mybranch ```

Once your branch is checked out, you can do all the development you want. After doing some development, to see the status of your changes enter 

``` git status ``` 

This will show you the changed files, and files that you can stage for commit. 

``` git add directory/moredirectory/filename ```

Or if you are lazy ``` git add -A ``` will stage all files unstaged for commit. Take care if you use this, you may stage files for commit that should not be 
staged. To stage all files in a folder for commit do ``` git add \directorytofolder ``` 

Once your changes are staged for commit, you can commit them with a message:

``` git commit -m "woweeeee" ```

At this point, the repository does not know about your local branch. In order to push
your branch to the repository, you need to use the --set-upstream switch:

``` git push --set-upstream origin mybranch ```

Once you've used the --set-upstream switch once, you can push any further changes as follows: 

``` git push ```

You can then go to the github repostiory website and request a pull request to merge the changes in to the main branch. Never push directly to main, or use 
force switches to ram your changes in. 

To get the current state of the main branch, you do not need to clone again, simply checkout the main branch, then do 

``` git pull ```

:) 

