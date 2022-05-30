# 🚧 WIP! Under Construction 🚧

## Automated Static Analysis Tool
This tool is aimed to perform static analysis code on C programming files.
It aims to automatically identify and fix:
- Unsafe use of strcpy (done)
- Sent pointers that arent checked (in progress)
- Uninitialised variables (not started)
- Unsafe use of strcpy for windows builds (not started)

## Usage
Only use for the WSOP PKCS#11 library that is written in C and has already been compiled with GCCv9.4.0.

Command:$ ./Helper <filename.c>

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