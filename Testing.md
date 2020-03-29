Testing is currently done only on internally available pipelines.

## Build pipeline

[![Build status](https://skype.visualstudio.com/SCC/_apis/build/status/SE-UP/azmi/build%20-%20azmi?branchName=master)](https://skype.visualstudio.com/SCC/_build/latest?definitionId=8166)

Build pipeline will create Debian package from source code.
It can be executed also on a dev branch.
Package will be uploaded to azure storage account as dev version.

Releasing official packages is done manually at the moment.

## Integration testing pipeline

[![Build status](https://skype.visualstudio.com/SCC/_apis/build/status/SE-UP/azmi/Integration%20-%20azmi?branchName=master)](https://skype.visualstudio.com/SCC/_build/latest?definitionId=8091)

Integration testing pipeline will take dev package build in previous step.
Pipeline will upload it to a new VM and execute [integration tests](./test/integration/integration_test.sh) against it.
It can also be executed against dev branch.


