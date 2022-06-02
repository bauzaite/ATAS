# 🚧 WIP! Under Construction! 🚧

## Automated Static Analysis Tool
This is not your average Static Analysis Tool. This tool has been tailored for team wildcat's
PKCS#11 library. It aims to identify and fix general mistakes developers make when making new changes
to the library. Meaning, the tool can actually achieve a higher rate of accuracy that most Static Analysis
Tools or Automated Static Analysis Tools since this tool knows:
- The exact style code would be written in.
- The defensive programming practices team aims to keep in the library and are not pointed out
by our existing analysis tools.
- The developer philosophy, meaning it will know what changes the developer would want
and can therefore apply an accurate fix.

This tool is aimed to perform static analysis code on C programming files.
It aims to automatically identify and fix:
- Unsafe use of strcpy (done) 
- Sent pointers that arent checked (done)
- Uninitialised variables (in progress)
- Unsafe use of strcpy for windows builds (not started)

## Usage
Only use for the WSOP PKCS#11 library that is written in C and has already been compiled with GCCv9.4.0.

1) Set flags inside the Helper.sh script

2) Run command:$ ./Helper <filename.c>

3) Press CTRL + F over <filename.c> and search for "☠" to see any suggested improvements by the tool.

## Example
Input:

void WSOPC_Function(char * dst){
	strcpy(dst, "hello");
}

Output:

void WSOPC_Function(char * dst){
	// ☠ if (NULL_PTR != dst){
		strcpy(dst, "hello"); // ☠ strncpy(dst, "hello", /*strlen(dst)*/)
	// ☠ }
}
