#!/bin/bash

if [ -z "$LAMBDASHARP" ]; then
    echo "ERROR: environment variable \$LAMBDASHARP is not set"
    exit 1
fi

if [ -z "$LAMBDASHARP_VERSION" ]; then
    echo "ERROR: environment variable \$LAMBDASHARP_VERSION is not set"
    exit 1
fi

if [ ! -d "$LAMBDASHARP/../Docs-LambdaSharpTool" ]; then
    echo "ERROR: could not locate 'Docs-LambdaSharpTool' folder"
    exit 1
fi

# remove generated metadata files
git clean -dxf "$LAMBDASHARP/Docs/sdk/"

# write current version to `version.txt`, which is used by docfx
echo "*** WRITING VERSION TO FILE: $LAMBDASHARP_VERSION"
echo $LAMBDASHARP_VERSION > "$LAMBDASHARP/Docs/version.txt"

# clean-out current documentation folder
echo "*** DELETING OLD DOCUMENTATION"
cd "$LAMBDASHARP/../Docs-LambdaSharpTool"

# restore original state
git clean -fd
git reset --hard origin/master

# make sure latest changes are present
git pull

# remove all tracked files
git ls-files -z | xargs -0 rm -f

# restore CNAME file, which is required by github
git checkout master -- CNAME

# generate new documentation
echo "*** GENERATING NEW DOCUMENTATION"
cd $LAMBDASHARP/Docs
docfx docfx.json
