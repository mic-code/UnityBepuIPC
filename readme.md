# Unity BepuPhysics InterProcessCommunication

This is a proof of concept showing bepu physics running in a seaparted process, sending data to unity for render
Using named pipe for rpc/synchonization and memory mapped file for transfering data

## How to run

1. Move Assets/Src folder somewhere outside the assets folder before opening unity editor, so the nuget package manager can compile and resolved required packages
2. Move the stuff back after unity editor finished opening with no error
3. Open Sim/Sim.sln and build the solution in release config (or debug if you check the debug switch in unity)
4. Play in Unity

## Controls

- Left click to shoot ball, which is rendered as brick, because everything is renderer as scaled cube regardless of their physical shape
- Press R to reset simulation

https://private-user-images.githubusercontent.com/26720201/389214194-a00727b4-cd19-4f86-85a5-e561be1e8c06.mp4?jwt=eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJnaXRodWIuY29tIiwiYXVkIjoicmF3LmdpdGh1YnVzZXJjb250ZW50LmNvbSIsImtleSI6ImtleTUiLCJleHAiOjE3MzIzODIxNDMsIm5iZiI6MTczMjM4MTg0MywicGF0aCI6Ii8yNjcyMDIwMS8zODkyMTQxOTQtYTAwNzI3YjQtY2QxOS00Zjg2LTg1YTUtZTU2MWJlMWU4YzA2Lm1wND9YLUFtei1BbGdvcml0aG09QVdTNC1ITUFDLVNIQTI1NiZYLUFtei1DcmVkZW50aWFsPUFLSUFWQ09EWUxTQTUzUFFLNFpBJTJGMjAyNDExMjMlMkZ1cy1lYXN0LTElMkZzMyUyRmF3czRfcmVxdWVzdCZYLUFtei1EYXRlPTIwMjQxMTIzVDE3MTA0M1omWC1BbXotRXhwaXJlcz0zMDAmWC1BbXotU2lnbmF0dXJlPTE4MzdlMjU1NTYwMWIyNjJjNTcyZTM0MTEyN2JkMzQxMzI4OGRkZDU5MWJkZjBjYjYyZTBhMGQ2ZWE3NGJlMTUmWC1BbXotU2lnbmVkSGVhZGVycz1ob3N0In0.V0wOp9gM-sGHKQvn6D5HykbLW84Pad1LsryDvR9xLBs