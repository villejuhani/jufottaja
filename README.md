# Jufottaja
Automates Jufo rating publication channels.
Uses the [Jufo REST-API](https://wiki.eduuni.fi/display/cscvirtajtp/Julkaisukanavatietokannan+REST-rajapinta).

## How
Give a csv file containing a table of publication channel names to Jufottaja and the program will:
- create a copy of the csv file (copy will be called jufotettu.csv)
- add a 'Jufo' column to the copy
  - which includes a result from trying to find a Jufo level:
    - A Jufo level (1-3)
    - NO LEVEL (i.e. Jufo level 0)
    - MULTIPLE POSSIBILITIES (you have to find the level manually)
    - FAILED (you have to find the level manually) 
 
Jufo level can be searched with these parameters:
- Publication channel name
- ISBN
- ISSN
- Conference Abbreviation

When running Jufottaja it will ask to specify which headers in the csv correspond to which of these search parameters. The csv can have 0..N amount of headers that correspond to each of the parameters. In the case there are multiple corresponding headers Jufottaja will search for the Jufo rating based on the first corresponding column that contains any text.

Requirements for the csv:
- CSV must be CSV (Comma Separeted Values), so use comma as the delimeter
- First row must be the table headers
  - The table header(s) that specify the column(s) that contain publication channel information must not have dashes (-)

## Instructions
### 1. Download
- Download for Windows (x64) from [Releases](https://github.com/villejuhani/jufottaja/releases).
- Extract the zip file.

### 2. Start the program
- Run the following command on the command line from the directory where the Jufottaja.exe is located:
  - `.\Jufottaja.exe C:\path\to\your.csv`
    - replace "C:\path\to\your.csv" with the csv's filepath that you want to Jufo-rate
   
### 3. Specify headers:
- Specify the headers in your file used for finding Publication Channel in Jufo. Do not use dash '-' in the headers. You have to specify the type aswell. The possible types are: name, isbn, issn, conferenceAbbreviation.
  - Example use: `-name: Publication Title, Conference Name -issn:ISSN`
 
The program will go through the csv file and create a copy of it with Jufo levels in the same location of the csv file specified at the start up.
