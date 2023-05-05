# Mako-IoT.Core.Configuration.MetadataGenerator
.NET Console application for generating metadata of your configuration API. This is a temporary solution to the fact, that nanoFramework's reflection doesn't fully support attributes.

## Config metadata workflow
1. Decorate your config model classes with SectionMetadata attributes.
2. Build
3. Launch MetadataGenerator
4. Expose generated metadata via configuration API.

TODO: example

## How to manually sync fork
- Clone repository and navigate into folder
- From command line execute bellow commands
- **git remote add upstream https://github.com/CShark-Hub/Mako-IoT.Base.git**
- **git fetch upstream**
- **git rebase upstream/main**
- If there are any conflicts, resolve them
  - After run **git rebase --continue**
  - Check for conflicts again
- **git push -f origin main**
