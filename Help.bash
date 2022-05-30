# Prerequisites:                          -    ╔══╗
# mono                                    -    ╚╗╔╝
# C# v7                                   -    ╔╝(¯`v´¯)
# ctag gets confused with gtest syntax    -    ╚══`.¸.Defensive Programming

#!/bin/bash
# set your flags!
FIX_STRCPY=1
INITALISE_ALL_VARIABLES=1
CHECK_POINTER_NOT_NULL=1
ADD_WINDOWS_STRCPY=1
FIXES_IN_COMMENT=1 # would you like chanages applied directly or in a comment next to the incorrect code? 

ctags --fields=+ne -o - --sort=yes $@ >> ctag.txt
mono v2.exe $@ $FIX_STRCPY $INITALISE_ALL_VARIABLES $CHECK_POINTER_NOT_NULL $ADD_WINDOWS_STRCPY $FIXES_IN_COMMENT
rm  ctag.txt
