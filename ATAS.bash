#!/bin/bash
# set your flags!
FIX_STRCPY=1
CHECK_POINTER_NOT_NULL=1
ADD_WINDOWS_STRNCPY=1
FIXES_IN_COMMENT=1 

ctags --fields=+ne -o - --sort=yes $@ >> ctag.txt
mono asat.exe $@ $FIX_STRCPY $CHECK_POINTER_NOT_NULL $ADD_WINDOWS_STRCPY $FIXES_IN_COMMENT
rm  ctag.txt
