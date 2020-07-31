# latest version

## version 0.5 

### date: July 29th, 2020
Released in scope of MSFT Hackathon 2020 event

Breaking changes:
* `setblobs` is not supporting `--container` parameter anymore, more info [here](docs/setblobs-container.md)

Functionality / performance
* added `setblobs` command
* `getblobs` speed increased 5 times!
* added `--file` output argument for `getcertificate` and `getsecret` subcommands
* updated to .NET core 3.1 and System.CommandLine 2.0
* `--exclude` accepts multiple values in `getblobs` and `setblobs`

Documentation / testing:
* extended unit testing for `azmi-main` project
* migrate deliverables to `azmi` storage account
* added [shields.io](https://shields.io) badges to readme page

[Full list of issues on GitHub](https://github.com/SRE-PRG/azmitool/milestone/5?closed=1)

# older versions

For technical list, published within debian package, take a look [here](debian/changelog).

## version 0.4.1

### date: June 19th, 2020

* Bug fix - azmi enters endless loop if no identity on the VM
* [Full list of issues on GitHub](https://github.com/SRE-PRG/azmitool/milestone/6?closed=1)

## version 0.4

### date April 16th, 2020

* Implemented new command: getcertificate
* Implemented new functionalities: fetch old versions of secrets and certificates, improved error handling
* [Full list of issues on GitHub](https://github.com/SRE-PRG/azmitool/milestone/4?closed=1)

## azmi (0.3.0-1) stable; urgency=medium

### date April 3rd, 2020

* Implemented new commands: getblobs, listblobs, getsecret
* Implemented new functionalities: JWT display of token, recursive folder copy, function delete-on-copy, function if-newer
* Internal improvements: documentation extended (examples, overview), commands split into separate classes
* [Full list of issues on GitHub](https://github.com/SRE-PRG/azmitool/milestone/3?closed=1)

## azmi (0.2.0-1) stable; urgency=medium

### date February 3rd, 2020

* Introduced new commandline parser
* Implemented getblob command
* Support VMs with multiple identities
* [Full list of issues on GitHub](https://github.com/SRE-PRG/azmitool/milestone/1?closed=1)

## azmi (0.1.0-1) stable; urgency=medium

### date January 6th, 2020

* Initial release
* Commands: gettoken, setblob
