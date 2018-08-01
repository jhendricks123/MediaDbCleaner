## Show help
```
C:\temp>MediaDbCleaner.exe --help
MediaDbCleaner 1.0.0.0
Copyright c  2018

  -p, --path        Required. Path to the root of the media database (the folder normally containing
                    cache.xml and archives_cache.xml)

  -r, --report      Save a report of media database tables identified as being in an invalid state (missing
                    important files/folders) to the specified path

  -l, --listonly    Log information about the state of the media database but do not make any modifications
                    to files or folders

  --help            Display this help screen.

  --version         Display version information.
```
## Example usage
```
c:\temp>MediaDbCleaner.exe -p "C:\MediaDatabase\b5d2b9c0-26a4-471b-b080-cfb77e02e465" -r results.txt
2018-08-01 00:11:34.580 - Starting. . .
2018-08-01 00:11:34.595 - Checking table '03ab3ed5-f8ff-4104-8862-7db784f7dcd6'
2018-08-01 00:11:34.595 - Checking table '13b90d70-ce27-49d0-b80c-6ba4431664d7'
2018-08-01 00:11:34.595 - Table with name '13b90d70-ce27-49d0-b80c-6ba4431664d7' contains no files.
2018-08-01 00:11:34.595 - Checking table '1c39bcac-aaad-49d7-85bc-c7d9468e3b7d'
2018-08-01 00:11:34.595 - Deleting table '13b90d70-ce27-49d0-b80c-6ba4431664d7'
2018-08-01 00:11:34.595 - Successfully deleted table '13b90d70-ce27-49d0-b80c-6ba4431664d7'
2018-08-01 00:11:34.595 - Checking table '919fc588-8e11-4af9-abf0-5bc7375a4c86'
2018-08-01 00:11:34.595 - Checking table '94ca4f88-a3c5-4de1-aed7-6d916b2c5343'
2018-08-01 00:11:34.595 - Checking table '9b7ee1e9-7e4e-4a2c-8a55-b6fd4102ff8c'
2018-08-01 00:11:34.595 - Checking table 'c0949c18-32d2-4697-a9d1-207a65b2ba17'
2018-08-01 00:11:34.595 - Checking table 'c467c819-3a30-4bb6-9526-fe5423803dc0'
2018-08-01 00:11:34.595 - Checking table 'd8ea9d2a-9361-4a8f-bd3c-4cb2db809f29'
2018-08-01 00:11:34.595 - Checking table 'f985fd44-5682-4ae2-afb5-ef2f5f330e19'
2018-08-01 00:11:34.595 - Operation completed.
```
