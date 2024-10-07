$(function () {
    var AwsS3 = Uppy.AwsS3;
    var $dragdroparea = $('#drag-drop-area');
    var $dragdroparea1 = $('#drag-drop-area-1');
    var allowedFileTypes = $dragdroparea.data('allowedfiletypes');
    var allowedFileTypes1 = $dragdroparea1.data('allowedfiletypes');
    var uppy;
    var uppy1;
    if ($dragdroparea.length>0) {
        uppy = Uppy.Core({
            id: 'uppyUpload', // use an id if you plan to use multiple Uppys (on different pages etc.)
            autoProceed: false, // if true the file uploads as soon as you drag it into the upload area, or select it - your user cannot edit file name or add metadata or tags - for this reason, we use 'false'
            restrictions: { // we can add restrictions here:
                maxFileSize: 1048576, //i.e. 1MB
                maxNumberOfFiles: 20,
                minNumberOfFiles: null, // if null, no minimum requirement
                allowedFileTypes: (allowedFileTypes ? allowedFileTypes.split(',') : null) // can use an array of extensions, i.e. ['.doc', '.docx', '.xls', '.xlsx']
            },
            logger: Uppy.debugLogger, // use this for debugging to see what's gone wrong
        })
            .use(Uppy.Dashboard, { // configure the Uppy Dashboard plugin...
                trigger: ".UppyModalOpenerBtn", // what we click to make Uppy dashboard modal appear - this is the class of our button, above
                inline: true, // if true, dashboard becomes part of layout rather than a modal - you can eliminate the button in that case
                closeModalOnClickOutside: true, // if true, we can click anywhere outside the modal to close it
                showLinkToFileUploadResult: false, // if true, a link to the uploaded file is generated and copies to the user's clipboard when clicked. Note the link won't contain credentials, and you'll need to configure your bucket's permissions and CORS settings to get this to work
                target: '#drag-drop-area', // this is the id of the <div> above - use a class instead of an id if multiple drag drop areas are required
                replaceTargetContent: false, // if true, removes all children of the target element - use this to put an upload form in place which disappears if Uppy appears - a fallback option
                showProgressDetails: true, // false shows % only while true shows % + MB of MB plus time left
                proudlyDisplayPoweredByUppy: true, // true shows 'Powered by Uppy' branding; false removes this. Attribution is nice...
                //note: "Images, Word, Excel, PDF, and similar files only, max 20 files of 30 MB each",
                height: 470, // height of the Dashboard in pixels - only applies if "inline: true" above - doesn't apply here since we are using the modal - have included here for reference only
                metaFields: [ // here we can include user editable fields, and we can use these elsewhere (i.e. upload as metadata, use as tags, use in our DB, etc.) Note that these don't seem to work out of the box - we need to add our own logic to make them work (notwithstanding the Uppy documentation...)
                    { id: 'name', name: 'Name', placeholder: 'You can rename the file here' }, // id is what we'll use to refer to this; name is what the user sees; placeholder is placeholder text
                    { id: 'caption', name: "Caption", placeholder: "Briefly describe what the file contains" }
                ],
                browserBackButtonClose: true // true allows the user to click the browser's back button to close the modal, rather than go back a page - this is a good idea!
            }).on('file-added', (file) => {
                uppy.setFileMeta(file.id, { id: GenerateUUID() });
            })
            .use(AwsS3, { // use the AwsS3 plugin                                  
                fields: [], // empty array 
                getUploadParameters:function(file) { // here we prepare our request to the server for the upload URL
                    var uppyS3Model = {
                        id: file.meta['id'],
                        filename: file.name, // here we are passing data to the server/back end
                        contenttype: file.type,
                        metadata: {
                            'name': file.meta['name'], // here we pass the 'name' variable to the back end, with 'file.meta['name']' referring to the 'name' from our metaFields id above
                            'caption': file.meta['caption'] // here we pass the 'caption' variable to the back end, with 'file.meta['caption']' referring to the 'caption' from our metaFields id above
                        }
                    };
                    return UppyS3UploadMethod(uppyS3Model).then(function (mediaS3Object) {
                        return mediaS3Object;
                    });
                },

            });
        uppy.on('complete', (result) => {
            if (result.successful) {
                addItemsGallary(result.successful);
            } else {
                console.log('Upload error: ', result.failed); // if upload failed, let's see what went wrong
            }
        });
    }
    if ($dragdroparea1.length > 0) {
        uppy1 = Uppy.Core({
            id: 'uppyUpload', // use an id if you plan to use multiple Uppys (on different pages etc.)
            autoProceed: false, // if true the file uploads as soon as you drag it into the upload area, or select it - your user cannot edit file name or add metadata or tags - for this reason, we use 'false'
            restrictions: { // we can add restrictions here:
                maxFileSize: 1048576, //i.e. 1MB
                maxNumberOfFiles: 20,
                minNumberOfFiles: null, // if null, no minimum requirement
                allowedFileTypes: (allowedFileTypes1 ? allowedFileTypes1.split(',') : null) // can use an array of extensions, i.e. ['.doc', '.docx', '.xls', '.xlsx']
            },
            logger: Uppy.debugLogger, // use this for debugging to see what's gone wrong
        })
            .use(Uppy.Dashboard, { // configure the Uppy Dashboard plugin...
                trigger: ".UppyModalOpenerBtn", // what we click to make Uppy dashboard modal appear - this is the class of our button, above
                inline: true, // if true, dashboard becomes part of layout rather than a modal - you can eliminate the button in that case
                closeModalOnClickOutside: true, // if true, we can click anywhere outside the modal to close it
                showLinkToFileUploadResult: false, // if true, a link to the uploaded file is generated and copies to the user's clipboard when clicked. Note the link won't contain credentials, and you'll need to configure your bucket's permissions and CORS settings to get this to work
                target: '#drag-drop-area-1', // this is the id of the <div> above - use a class instead of an id if multiple drag drop areas are required
                replaceTargetContent: false, // if true, removes all children of the target element - use this to put an upload form in place which disappears if Uppy appears - a fallback option
                showProgressDetails: true, // false shows % only while true shows % + MB of MB plus time left
                proudlyDisplayPoweredByUppy: true, // true shows 'Powered by Uppy' branding; false removes this. Attribution is nice...
                //note: "Images, Word, Excel, PDF, and similar files only, max 20 files of 30 MB each",
                height: 470, // height of the Dashboard in pixels - only applies if "inline: true" above - doesn't apply here since we are using the modal - have included here for reference only
                metaFields: [ // here we can include user editable fields, and we can use these elsewhere (i.e. upload as metadata, use as tags, use in our DB, etc.) Note that these don't seem to work out of the box - we need to add our own logic to make them work (notwithstanding the Uppy documentation...)
                    { id: 'name', name: 'Name', placeholder: 'You can rename the file here' }, // id is what we'll use to refer to this; name is what the user sees; placeholder is placeholder text
                    { id: 'caption', name: "Caption", placeholder: "Briefly describe what the file contains" }
                ],
                browserBackButtonClose: true // true allows the user to click the browser's back button to close the modal, rather than go back a page - this is a good idea!
            }).on('file-added', (file) => {
                uppy1.setFileMeta(file.id, { id: GenerateUUID() });
            })
            .use(AwsS3, { // use the AwsS3 plugin                                  
                fields: [], // empty array 
                getUploadParameters:function(file) { // here we prepare our request to the server for the upload URL
                    var uppyS3Model = {
                        id: file.meta['id'],
                        filename: file.name, // here we are passing data to the server/back end
                        contenttype: file.type,
                        metadata: {
                            'name': file.meta['name'], // here we pass the 'name' variable to the back end, with 'file.meta['name']' referring to the 'name' from our metaFields id above
                            'caption': file.meta['caption'] // here we pass the 'caption' variable to the back end, with 'file.meta['caption']' referring to the 'caption' from our metaFields id above
                        }
                    };
                    return UppyS3UploadMethod(uppyS3Model).then(function (mediaS3Object) {
                        return mediaS3Object;
                    });
                },

            });
        uppy1.on('complete', (result) => {
            if (result.successful) {
                addItemsMasonryGallary(result.successful);
            } else {
                console.log('Upload error: ', result.failed); // if upload failed, let's see what went wrong
            }
        });
    }
    $("#btnResetUppy").click(function () {
        if (uppy)
            uppy.reset();
        if (uppy1)
            uppy1.reset();
    });
    function UppyS3UploadMethod(req) {
        var dfd = new $.Deferred();
        AWS.config.update({
            region: $s3BucketRegion,
            credentials: new AWS.CognitoIdentityCredentials({
                IdentityPoolId: $s3IdentityPoolId
            })
        });
        var s3 = new AWS.S3();

        //const myBucket = $s3BucketName; // the bucket you'll be uploading to


        var metaName = ""; // we're going to use the user input file name as metadata, and if the user hasn't input anything, we'll use the original file name. Here we prepare the variable
        if (req.metadata.name === "") { // here we test if the user has input anything in the 'name' field - this is the convoluted way we need to extract the name from the passed stringified JSON object - if there's a better way to do this, I'm all ears...
            metaName = req.filename; // if the user hasn't made any changes, just use the original filename
        } else {
            metaName = req.metadata.name; // otherwise, use whatever the user has input (this should probably be sanitized...)
        }
        var metaCaption = ""; // set up a variable to capture whatever the user input in the 'caption' field
        if (!req.metadata.caption) { // if there is no caption...
            metaCaption = ""; // set it blank
        } else {
            metaCaption = req.metadata.caption; // otherwise, use what the user input
        }


        const params = { // now let's set up our parameters for the pre-signed key...
            Metadata: { // here we're adding metadata. The key difference between metadata and tags is that tags can be changed - metadata cannot!
                'file-name': metaName, // add the user-input filename as the value for the 'fileName' metadata key
                'caption': metaCaption, // add the user-input caption as the value for the 'caption' metadata key
                'file-type': req.contenttype, // let's grab the user who uploaded this and use the username as the value with the 'user' key
                'upload-date-utc': Date(), // and let's grab the UTC date the file was uploaded
                "file-extension": metaName.split('.').pop(),
            },
            Bucket: $s3BucketName, // our AWS S3 upload bucket, from way above...
            Key: req.id,
            ContentType: req.contenttype,
            ACL: "bucket-owner-full-control"
        };
        s3.getSignedUrl('putObject', params, (err, url) => { // get the pre-signed URL from AWS - if you alter this URL, it will fail          

            dfd.resolve({ // send info back to the client/front end
                method: 'put', // our upload method
                url: url, // variable to hold the URL
                fields: {}, // leave this an empty object
                headers: { 'caption': metaCaption } // here we add the tags we created above
            });
        })


        return dfd.promise();
    };
});


