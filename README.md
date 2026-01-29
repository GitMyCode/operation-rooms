# operation-rooms

Console application that reads an ODS file describing operation slots and outputs a CSV of scheduled operations.

## Usage (Windows)

### Option 1: Double‑click the .exe
1. Double‑click `operation-rooms.exe`.
2. A console window opens and prompts for two paths:
	 - Input ODS file path (e.g., `C:\Users\You\Desktop\OperationTimeSlot.ods`)
	 - Output CSV file path (e.g., `C:\Users\You\Desktop\result.csv`)
3. Paste each path and press Enter.
4. The app runs and writes the CSV at the path you provided.

### Option 2: Run from Command Prompt
```bat
operation-rooms.exe
```
Then follow the prompts as above.

### Sample input file
The repo contains a sample ODS file you can test with:
- `OperationTimeSlot.ods`

## Output
- Results are written to the CSV path you provide.
- A log file is written next to the executable with the same name and a `.log` extension.
	- Example: `operation-rooms.log`
	- The log file is size‑based and rolls when it reaches ~5 MB.

## Notes
- If you double‑click the exe, the console stays open waiting for input.
- If you close the console before entering paths, no output will be written.