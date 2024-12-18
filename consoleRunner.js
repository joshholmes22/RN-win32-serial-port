const {spawn} = require('child_process');
const path = require('path');

console.log('Resolved console app path:', consoleAppPath);

// Path to the console app
const consoleAppPath = path.resolve(
  __dirname,
  './native-resources/ConsoleApp1.exe',
);

// Start the console app process
console.log(`Starting console app from path: ${consoleAppPath}`);

try {
  const process = spawn(consoleAppPath, [], {shell: true});

  // Capture standard output
  process.stdout.on('data', data => {
    console.log(`Output: ${data.toString()}`);
  });

  // Capture standard error
  process.stderr.on('data', data => {
    console.error(`Error: ${data.toString()}`);
  });

  // Handle exit
  process.on('close', code => {
    console.log(`Console app exited with code: ${code}`);
  });

  // Handle errors when starting the process
  process.on('error', err => {
    console.error(`Failed to start console app: ${err.message}`);
  });
} catch (error) {
  console.error(`Exception while running the console app: ${error.message}`);
}
