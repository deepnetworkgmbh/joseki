## MACOS USERS

If you want to run the bash scripts on MACOS, please use the following libraries

brew install coreutils
brew install gnu-sed

# Modify your path
PATH="/usr/local/opt/coreutils/libexec/gnubin:$PATH"
PATH="/usr/local/opt/gnu-sed/libexec/gnubin:$PATH"

You can remove PATH modifications after the installation. 
