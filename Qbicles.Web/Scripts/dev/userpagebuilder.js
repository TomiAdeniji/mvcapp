// Declare a global object to store view data.
var _pageBuilder;

_pageBuilder = {
    Id: 0,
    PageTitle: '',
    Status: 'IsDraft',
    BlockHeroes: [],
    BlockTextImages: [],
    BlockGalleries: [],
    BlockMasonryGalleries: []
};
//Data-Bind
$(function () {
    // Update the viewData object with the current field keys and values.
    function updateViewData(key, value) {
        var obj = key.split(".");
        if (obj.length == 1)
            _pageBuilder[key] = value;
        else {
            var model = _pageBuilder[obj[0]];//Get Data Model
            model[obj[1]] = value;//Set value 
        }
    }

    // Register all bindable elements
    function detectBindableElements() {
        var bindableEls;

        bindableEls = $('[data-bind]');

        // Add event handlers to update viewData and trigger callback event.
        bindableEls.on('change', function () {
            var $this;

            $this = $(this);

            updateViewData($this.data('bind'), $this.val());
            //$(document).trigger('updateDisplay');
        });
    }

    // Trigger this event to manually update the list of bindable elements, useful when dynamically loading form fields.
    $(document).on('updateBindableElements', detectBindableElements);

    detectBindableElements();
    var pageId = $('#hdfPageID').val();
    if (pageId != 0) {
        hideUpDownBlock();
        $('#btnAddBlock').attr('disabled',true);
        getPageBuilderById(pageId);
    }
    initPuginsGalary();
});
function initPuginsGalary() {
    $('#page_templates_builder .nvgalleryowl').owlCarousel({
        loop: true,
        nav: false,
        dots: true,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true,
        margin: 30,
        responsive: {
            0: {
                items: 1
            },
            768: {
                items: 2
            },
            1400: {
                items: 3
            }
        }
    });
}
//end Data-Bind
function saveUserPageBuilder(status) {
    _pageBuilder.Status = status;
    if ($('#frmpageBuilder').valid()) {
        $.LoadingOverlay("show");
        $.each(_pageBuilder.BlockHeroes, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockTextImages, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        
        $.each(_pageBuilder.BlockGalleries, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockMasonryGalleries, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.ajax({
            type: 'POST',
            url: '/ProfilePage/SaveUserPageBuilder',
            data: { pageBuilder: _pageBuilder },
            success: function (response) {
                if (response.result) {
                    if (status == 'IsActive') {
                        $('#btnDiscardChange').attr('disabled',true);
                    }
                    $('#hdfPageID').val(response.Object).trigger('change');
                    cleanBookNotification.success(_L("UPDATE_MSG_SUCCESS"), "Qbicles");
                }
                else if (!response.result && response.msg) {
                    cleanBookNotification.error(response.msg, "Qbicles");
                } else {
                    cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
                }
            },
            error: function (xhr, textStatus, error) {
                cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
            }
        }).always(function () {
            LoadingOverlayEnd();
        });
    }

}
function addBlock(blockId) {
    $('#btnDiscardChange').removeAttr('disabled');
    var elmClone = $(blockId).clone()
    var $blockNew = $(elmClone);
    if ($blockNew.length > 0) {
        var idCloneBlock = UniqueId(5);
        $blockNew.attr('id', idCloneBlock);
        var type = $blockNew.data('type');
        $('#newstart').hide();
        $('#page_templates_builder').append($blockNew);
        $('#' + idCloneBlock + ' button.btn-edit-block').attr('onclick', 'bindDataForBlock(\'' + idCloneBlock + '\',\'' + type + '\')');
        $('#' + idCloneBlock + ' button.btn-delete-block').attr('onclick', 'removeBlock(\'' + idCloneBlock + '\')');
        $('#' + idCloneBlock + ' button.moveup').attr('onclick', 'moveUpDown(\'' + idCloneBlock + '\',true)');
        $('#' + idCloneBlock + ' button.movedown').attr('onclick', 'moveUpDown(\'' + idCloneBlock + '\',false)');
        $('#' + idCloneBlock).fadeIn();
        $('#' + idCloneBlock + ' .nvgalleryowl').owlCarousel({
            loop: true,
            nav: false,
            dots: true,
            autoplay: true,
            autoplayTimeout: 3000,
            autoplayHoverPause: true,
            margin: 30,
            responsive: {
                0: {
                    items: 1
                },
                768: {
                    items: 2
                },
                1400: {
                    items: 3
                }
            }
        });
        //$('#' + idCloneBlock + ' .masonry').masonry();
        $('#' + idCloneBlock + ' .masonry').masonry({
            columnWidth: 420,
            isFitWidth: true,
            itemSelector: '.masonitem'
        });
        initDefaultBlockData(type, idCloneBlock);
        hideUpDownBlock();
    }
}
function initDefaultBlockData(type, elmid) {
    var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
    switch (type) {
        case 'HeroPersonal':
            var hero = {
                ElmId: elmid,
                Id: 0,
                HeroBackgroundType: 'SingleColour',
                HeroBackgroundColour: '#60a7bd',
                HeroGradientColour1: '#1dd8cf',
                HeroGradientColour2: '#8845f0',
                HeroBackGroundImage: '',
                HeroHeadingText: 'I\'m<span>Sarah</span>',
                HeroHeadingFontSize: 58,
                HeroHeadingColour: '#ffffff',
                HeroHeadingAccentColour: '#d0d265',
                HeroSubHeadingText: 'I\'m an amateur photographer and go- getter who\'s just dying to show off my latest snaps.',
                HeroSubHeadingFontSize: 24,
                HeroSubHeadingColour: '#ffffff',
                HeroFeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/happy.jpg',
                HeroIsIncludeButton: true,
                HeroButtonText: 'Tell me more',
                HeroButtonColour: '#4ccba9',
                HeroButtonJumpTo: 0,
                HeroExternalLink: '',
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'HeroPersonal',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockHeroes.push(hero);
            break;
        case 'TextAndImage':
            var textAndImage = {
                ElmId: elmid,
                Id: 0,
                HeadingText: 'Text panel <span>with image</span>',
                HeadingFontSize: 32,
                HeadingColour: '#224157',
                HeadingAccentColour: '#d0d265',
                Content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Donec commodo augue a metus interdum, ac tincidunt sapien semper. Nam consequat lorem a quam viverra, eu ornare eros condimentum. Aenean pulvinar vitae ligula vitae vulputate. Morbi odio eros, tincidunt in luctus eget, facilisis a ante. Etiam lobortis erat at libero ultricies ultrices.',
                ContentFontSize: 16,
                ContentColour: '#333333',
                IsIncludeButton: true,
                ButtonText: 'Button example',
                ButtonColour: '#2990ea',
                ButtonJumpTo: 0,
                ExternalLink: '',
                FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/thinking.jpg',
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'TextImage',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockTextImages.push(textAndImage);
            break;
        case 'Gallery':
            var gallery = {
                ElmId: elmid,
                Id: 0,
                GalleryItems: [{
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/bee.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 1
                },
                {
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/tree.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 2
                },
                {
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/cow.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 3
                },
                {
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/woman.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 4
                }],
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'Gallery',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockGalleries.push(gallery);
            break;
        case 'MasonryGallery':
            var masonryGallery = {
                Id: 0,
                ElmId: elmid,
                HeadingText:'My stuff',
                HeadingFontSize: 32,
                HeadingColour: '#000000',
                HeadingAccentColour: '#4ccba9',
                SubHeadingText: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nullam facilisis dui at turpis laoreet vestibulum. In pretium libero diam!',
                SubHeadingFontSize: 16,
                SubHeadingColour: '#889aa7',
                GalleryItems: [{
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry7.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 1
                },
                {
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry1.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 2
                },
                {
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry2.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 3
                },
                {
                    Id: 0,
                    FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry3.jpg',
                    Title: 'Gallery item title',
                    Content: 'Image caption content displayed here',
                    DisplayOrder: 4
                    },
                    {
                        Id: 0,
                        FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry4.jpg',
                        Title: 'Gallery item title',
                        Content: 'Image caption content displayed here',
                        DisplayOrder: 5
                    },
                    {
                        Id: 0,
                        FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry5.jpg',
                        Title: 'Gallery item title',
                        Content: 'Image caption content displayed here',
                        DisplayOrder: 6
                    },
                    {
                        Id: 0,
                        FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry6.jpg',
                        Title: 'Gallery item title',
                        Content: 'Image caption content displayed here',
                        DisplayOrder: 7
                    },
                    {
                        Id: 0,
                        FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/masonry8.jpg',
                        Title: 'Gallery item title',
                        Content: 'Image caption content displayed here',
                        DisplayOrder: 8
                    }
                ]
            };
            _pageBuilder.BlockMasonryGalleries.push(masonryGallery);
            break;
        default:
            break
    }
}
function bindDataForBlock(blockId, type) {
    switch (type) {
        case 'HeroPersonal':
            var modalId = '#pages-blocks-hero-edit';
            var hero = _pageBuilder.BlockHeroes.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (hero && !hero.ElmId)
                hero.ElmId = ('block' + hero.Id);
            else if (!hero)
                return;
            var bgtype = '';
            if (hero.HeroBackgroundType === 'SingleColour')
                bgtype = '#bgcolour';
            else if (hero.HeroBackgroundType === 'Gradient')
                bgtype = '#bggradient';
            else if (hero.HeroBackgroundType === 'Image')
                bgtype = '#bgimage';
            $(modalId + ' select[name=HeroBackgroundType]').val(bgtype).change();
            $(modalId + ' input[name=HeroBackgroundColour]').val(hero.HeroBackgroundColour);
            $(modalId + ' input[name=HeroGradientColour1]').val(hero.HeroGradientColour1);
            $(modalId + ' input[name=HeroGradientColour2]').val(hero.HeroGradientColour2);
            $(modalId + ' input[name=HeroHeadingAccentColour]').val(hero.HeroHeadingAccentColour);
            $(modalId + ' input[name=HeroBackGroundImage]').val(hero.HeroBackGroundImage);
            $(modalId + ' input[name=HeroHeadingText]').val(hero.HeroHeadingText);
            $(modalId + ' input[name=HeroHeadingFontSize]').val(hero.HeroHeadingFontSize);
            $(modalId + ' input[name=HeroHeadingColour]').val(hero.HeroHeadingColour);
            $(modalId + ' textarea[name=HeroSubHeadingText]').val(hero.HeroSubHeadingText);
            $(modalId + ' input[name=HeroSubHeadingFontSize]').val(hero.HeroSubHeadingFontSize);
            $(modalId + ' input[name=HeroSubHeadingColour]').val(hero.HeroSubHeadingColour);
            $(modalId + ' input[name=HeroFeaturedImage]').val(hero.HeroFeaturedImage);
            $(modalId + ' .preview-HeroFeaturedImage').attr('src', getUrlImage(hero.HeroFeaturedImage));
            getBlocksForJumbTo(modalId + ' select[name=ButtonJumpTo]', blockId);
            $(modalId + ' select[name=ButtonJumpTo]').val((hero.HeroButtonJumpTo == '0' ? 'external' : hero.HeroButtonJumpTo)).change();
            $(modalId + ' input[name=ExternalLink]').val(hero.HeroExternalLink);
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            $('#drag-drop-area').data('blockid', blockId);
            $(modalId).modal('show');
            break;
        case 'TextAndImage':
            var modalId = '#pages-blocks-textimg-edit';
            var textAndImage = _pageBuilder.BlockTextImages.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (textAndImage && !textAndImage.ElmId)
                textAndImage.ElmId = ('block' + textAndImage.Id);
            else if (!textAndImage)
                return;
            $(modalId + ' input[name=HeadingAccentColour]').val(textAndImage.HeadingAccentColour);
            $(modalId + ' input[name=HeadingText]').val(textAndImage.HeadingText);
            $(modalId + ' input[name=HeadingFontSize]').val(textAndImage.HeadingFontSize);
            $(modalId + ' input[name=HeadingColour]').val(textAndImage.HeadingColour);
            $(modalId + ' input[name=Content]').val(textAndImage.Content);
            $(modalId + ' input[name=ContentFontSize]').val(textAndImage.ContentFontSize);
            $(modalId + ' input[name=ContentColour]').val(textAndImage.ContentColour);
            $(modalId + ' input[name=IsIncludeButton]').prop('checked', textAndImage.IsIncludeButton);
            $(modalId + ' input[name=ButtonText]').val(textAndImage.ButtonText);
            $(modalId + ' input[name=ButtonColour]').val(textAndImage.ButtonColour);
            getBlocksForJumbTo(modalId + ' select[name=ButtonJumpTo]', blockId);
            $(modalId + ' select[name=ButtonJumpTo]').val((textAndImage.ButtonJumpTo == '0' ? 'external' : textAndImage.ButtonJumpTo)).change();
            $(modalId + ' input[name=ExternalLink]').val(textAndImage.ExternalLink);
            $(modalId + ' input[name=FeaturedImage]').val(textAndImage.FeaturedImage);
            $(modalId + ' .preview-FeaturedImage').attr('src', getUrlImage(textAndImage.FeaturedImage));
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            $(modalId).modal('show');
            break;
        case 'Gallery':
            var modalId = '#pages-blocks-gallery-edit';
            var gallery = _pageBuilder.BlockGalleries.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (gallery && !gallery.ElmId)
                gallery.ElmId = ('block' + gallery.Id);
            else if (!gallery)
                return;
            $('#gallery-items').empty();
            if (gallery.GalleryItems.length > 0) {
                $('#gallery-intro').hide();
                $('#gallery-items').show();
            } else {
                $('#gallery-intro').show();
                $('#gallery-items').hide();
            }
            $.each(gallery.GalleryItems, function (index, value) {
                var elmClone = $('#gallery-item-block').clone();
                var $galleryitem = $(elmClone);
                $galleryitem.show();
                var itemID = 'item-' + UniqueId(5);
                $('#gallery-items').append($galleryitem);
                $galleryitem.attr('id', itemID);
                $galleryitem.find('input[name=Title]').val(value.Title);
                $galleryitem.find('textarea[name=Content]').val(value.Content);
                $galleryitem.find('input[name=FeaturedImage]').val(value.FeaturedImage);
                $galleryitem.find('div.preview').css('background-image', 'url(' + getUrlImage(value.FeaturedImage) + ')');
                $galleryitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'gallery-items\');');
                $galleryitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'gallery-items\')');
                $galleryitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'gallery-items\')');
            });
            hideUpDownFirstLastItem('gallery-items');
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            $("#btnResetUppy").click();
            $(modalId).modal('show');
            break;
        case 'MasonryGallery':
            var modalId = '#pages-blocks-masonry-edit';
            var gallery = _pageBuilder.BlockMasonryGalleries.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (gallery && !gallery.ElmId)
                gallery.ElmId = ('block' + gallery.Id);
            else if (!gallery)
                return;
            $('#masonry-items').empty();
            if (gallery.GalleryItems.length > 0) {
                $('#masonry-intro').hide();
                $('#masonry-items').show();
            } else {
                $('#masonry-intro').show();
                $('#masonry-items').hide();
            }
            $.each(gallery.GalleryItems, function (index, value) {
                var elmClone = $('#gallery-item-block').clone();
                var $galleryitem = $(elmClone);
                $galleryitem.show();
                var itemID = 'item-' + UniqueId(5);
                $('#masonry-items').append($galleryitem);
                $galleryitem.attr('id', itemID);
                $galleryitem.find('input[name=Title]').val(value.Title);
                $galleryitem.find('textarea[name=Content]').val(value.Content);
                $galleryitem.find('input[name=FeaturedImage]').val(value.FeaturedImage);
                $galleryitem.find('div.preview').css('background-image', 'url(' + getUrlImage(value.FeaturedImage) + ')');
                $galleryitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'gallery-items\');');
                $galleryitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'gallery-items\')');
                $galleryitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'gallery-items\')');
            });
            hideUpDownFirstLastItem('masonry-items');
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            $("#btnResetUppy").click();
            $(modalId).modal('show');
            break;
        default:
            break;
    }
}
function updateDataBlock(blockId, type) {
    $('#btnDiscardChange').removeAttr('disabled');
    switch (type) {
        case 'HeroPersonal':
            var modalId = '#pages-blocks-hero-edit';
            var hero = _pageBuilder.BlockHeroes.find(x => x.ElmId == blockId);
            var bgtype = $(modalId + ' select[name=HeroBackgroundType]').val();
            if (bgtype === '#bgcolour')
                hero.HeroBackgroundType = 'SingleColour';
            else if (bgtype === '#bggradient')
                hero.HeroBackgroundType = 'Gradient';
            else if (bgtype === '#bgimage')
                hero.HeroBackgroundType = 'Image';
            hero.HeroBackgroundColour = $(modalId + ' input[name=HeroBackgroundColour]').val();
            hero.HeroGradientColour1 = $(modalId + ' input[name=HeroGradientColour1]').val();
            hero.HeroGradientColour2 = $(modalId + ' input[name=HeroGradientColour2]').val();
            hero.HeroHeadingAccentColour = $(modalId + ' input[name=HeroHeadingAccentColour]').val();
            hero.HeroBackGroundImage = $(modalId + ' input[name=HeroBackGroundImage]').val();
            hero.HeroHeadingText = $(modalId + ' input[name=HeroHeadingText]').val();
            hero.HeroHeadingFontSize = $(modalId + ' input[name=HeroHeadingFontSize]').val();
            hero.HeroHeadingColour = $(modalId + ' input[name=HeroHeadingColour]').val();
            hero.HeroSubHeadingText = $(modalId + ' textarea[name=HeroSubHeadingText]').val();
            hero.HeroSubHeadingFontSize = $(modalId + ' input[name=HeroSubHeadingFontSize]').val();
            hero.HeroSubHeadingColour = $(modalId + ' input[name=HeroSubHeadingColour]').val();
            hero.HeroFeaturedImage = $(modalId + ' input[name=HeroFeaturedImage]').val();
            hero.HeroIsIncludeButton = $(modalId + ' input[name=IsIncludeButton]').prop('checked');
            hero.HeroButtonText = $(modalId + ' input[name=ButtonText]').val();
            hero.HeroButtonColour = $(modalId + ' input[name=ButtonColour]').val();
            var btnjumptoval = $(modalId + ' select[name=ButtonJumpTo]').val();
            hero.HeroButtonJumpTo = (btnjumptoval == 'external' ? 0 : btnjumptoval);
            hero.HeroExternalLink = $(modalId + ' input[name=ExternalLink]').val();
            updateBlockPreview(blockId, hero, type);
            $(modalId).modal('hide');
            break;
        case 'TextAndImage':
            var modalId = '#pages-blocks-textimg-edit';
            var textAndImage = _pageBuilder.BlockTextImages.find(x => x.ElmId == blockId);
            textAndImage.HeadingAccentColour = $(modalId + ' input[name=HeadingAccentColour]').val();
            textAndImage.HeadingText = $(modalId + ' input[name=HeadingText]').val();
            textAndImage.HeadingFontSize = $(modalId + ' input[name=HeadingFontSize]').val();
            textAndImage.HeadingColour = $(modalId + ' input[name=HeadingColour]').val();
            textAndImage.Content = $(modalId + ' textarea[name=Content]').val();
            textAndImage.ContentFontSize = $(modalId + ' input[name=ContentFontSize]').val();
            textAndImage.ContentColour = $(modalId + ' input[name=ContentColour]').val();
            textAndImage.IsIncludeButton = $(modalId + ' input[name=IsIncludeButton]').prop('checked');
            textAndImage.ButtonText = $(modalId + ' input[name=ButtonText]').val();
            textAndImage.ButtonColour = $(modalId + ' input[name=ButtonColour]').val();
            var btnjumptoval = $(modalId + ' select[name=ButtonJumpTo]').val();
            textAndImage.ButtonJumpTo = (btnjumptoval == 'external' ? 0 : btnjumptoval);
            textAndImage.ExternalLink = $(modalId + ' input[name=ExternalLink]').val();
            textAndImage.FeaturedImage = $(modalId + ' input[name=FeaturedImage]').val();
            updateBlockPreview(blockId, textAndImage, type);
            $(modalId).modal('hide');
            break;
        case 'Gallery':
            var modalId = '#pages-blocks-gallery-edit';
            var gallery = _pageBuilder.BlockGalleries.find(x => x.ElmId == blockId);
            var $items = $('#gallery-items div.item-block');
            gallery.GalleryItems = [];
            $items.each(function (index, element) {
                var $galleryitem = $(element);
                var data = {
                    Id: 0,
                    FeaturedImage: $galleryitem.find('input[name=FeaturedImage]').val(),
                    Title: $galleryitem.find('input[name=Title]').val(),
                    Content: $galleryitem.find('textarea[name=Content]').val(),
                    DisplayOrder: (index + 1)
                };
                gallery.GalleryItems.push(data);
            });
            updateBlockPreview(blockId, gallery, type);
            $(modalId).modal('hide');
            break;
        case 'MasonryGallery':
            var modalId = '#pages-blocks-masonry-edit';
            var masonrygallery = _pageBuilder.BlockMasonryGalleries.find(x => x.ElmId == blockId);
            masonrygallery.HeadingAccentColour = $(modalId + ' input[name=HeadingAccentColour]').val();
            masonrygallery.HeadingText = $(modalId + ' input[name=HeadingText]').val();
            masonrygallery.HeadingFontSize = $(modalId + ' input[name=HeadingFontSize]').val();
            masonrygallery.HeadingColour = $(modalId + ' input[name=HeadingColour]').val();
            masonrygallery.SubHeadingText = $(modalId + ' textarea[name=SubHeadingText]').val();
            masonrygallery.SubHeadingFontSize = $(modalId + ' input[name=SubHeadingFontSize]').val();
            masonrygallery.SubHeadingColour = $(modalId + ' input[name=SubHeadingColour]').val();
            var $items = $('#masonry-items div.item-block');
            masonrygallery.GalleryItems = [];
            $items.each(function (index, element) {
                var $galleryitem = $(element);
                var data = {
                    Id: 0,
                    FeaturedImage: $galleryitem.find('input[name=FeaturedImage]').val(),
                    Title: $galleryitem.find('input[name=Title]').val(),
                    Content: $galleryitem.find('textarea[name=Content]').val(),
                    DisplayOrder: (index + 1)
                };
                masonrygallery.GalleryItems.push(data);
            });
            updateBlockPreview(blockId, masonrygallery, type);
            $(modalId).modal('hide');
            break;
        default:
            break;
    }
}
function updateBlockPreview(blockId, objModel, type) {
    var blockId = '#' + blockId;
    switch (type) {
        case 'HeroPersonal':
            if (objModel.HeroBackgroundType == 'Gradient') {
                $(blockId).css('background', 'linear-gradient(140deg, ' + objModel.HeroGradientColour1 + ' 0%, ' + objModel.HeroGradientColour2 + ' 100%)');
            } else if (objModel.HeroBackgroundType == 'SingleColour')
            {    $(blockId).css('background', 'none');
                $(blockId).css('background-color', objModel.HeroBackgroundColour);
            } else if (objModel.HeroBackgroundType == 'Image') {
                $(blockId).css('background-image', "url(" + getUrlImage(objModel.HeroBackGroundImage) + ")");
                $(blockId).css('-webkit-background-size', 'cover');
                $(blockId).css('-moz-background-size', 'cover');
                $(blockId).css('-ms-background-size', 'cover');
                $(blockId).css('-o-background-size', 'cover');
                $(blockId).css('background-size', 'cover');
            }
            var $h1heading = $(blockId + ' h1.block-heading');
            $h1heading.html(objModel.HeroHeadingText);
            $h1heading.css('font-size', objModel.HeroHeadingFontSize + 'px');
            $h1heading.css('color', objModel.HeroHeadingColour);
            var $h1headingAccentColour = $(blockId + ' h1.block-heading span');
            $h1headingAccentColour.css('color', objModel.HeroHeadingAccentColour);
            var $h2heading = $(blockId + ' h2.block-subheading');
            $h2heading.html(objModel.HeroSubHeadingText);
            $h2heading.css('font-size', objModel.HeroSubHeadingFontSize + 'px');
            $h2heading.css('color', objModel.HeroSubHeadingColour);
            $(blockId + ' img.nvpfeature').attr('src', getUrlImage(objModel.HeroFeaturedImage));
            var $button = $(blockId + ' button.nvpbutton');
            if (objModel.HeroIsIncludeButton) {
                $button.show();
            } else {
                $button.hide();
            }
            $button.text(objModel.HeroButtonText);
            $button.css('background-color', objModel.HeroButtonColour);
            if (objModel.HeroButtonJumpTo == 0)
                $button.attr('onclick', 'location.href="https://' + objModel.HeroExternalLink + '"');
            else
                $button.attr('onclick', 'location.href="' + objModel.HeroButtonJumpTo + '"');
            break;
        case 'TextAndImage':
            var $h1heading = $(blockId + ' h1.block-heading');
            $h1heading.html(objModel.HeadingText);
            $h1heading.css('font-size', objModel.HeadingFontSize + 'px');
            $h1heading.css('color', objModel.HeadingColour);
            var $h1headingAccentColour = $(blockId + ' h1.block-heading span');
            $h1headingAccentColour.css('color', objModel.HeadingAccentColour);
            var $content = $(blockId + ' p.block-content');
            $content.html(objModel.Content);
            $content.css('font-size', objModel.ContentFontSize + 'px');
            $content.css('color', objModel.ContentColour);
            var $button = $(blockId + ' button.nvbutton');
            if (objModel.IsIncludeButton) {
                $button.show();
            } else {
                $button.hide();
            }
            $button.text(objModel.ButtonText);
            $button.css('background-color', objModel.ButtonColour);
            if (objModel.ButtonJumpTo == 0)
                $button.attr('onclick', 'location.href="https://' + objModel.ExternalLink + '"');
            else
                $button.attr('onclick', 'location.href="' + objModel.ButtonJumpTo + '"');
            $(blockId + ' div.block-img').css('background-image', "url(" + getUrlImage(objModel.FeaturedImage) + ")");
            break;
        case 'Gallery':
            var $gallerylist = $(blockId + ' div.nvgalleryowl');
            $gallerylist.empty();
            $.each(objModel.GalleryItems, function (index, value) {
                var _blockfeatureHtml = '<div class="item">';
                _blockfeatureHtml += '<div class="nvgalleryitem" style="background-image: url(\'' + getUrlImage(value.FeaturedImage) + '\');">';
                _blockfeatureHtml += '<div class="nvcaption">';
                _blockfeatureHtml += '<h5>' + value.Title + '</h5>';
                _blockfeatureHtml += '<p>' + value.Content + '</p>';
                _blockfeatureHtml += '</div></div></div>';
                $gallerylist.append(_blockfeatureHtml);
            });
            $gallerylist.owlCarousel('destroy');
            $gallerylist.owlCarousel({
                loop: true,
                nav: false,
                dots: true,
                autoplay: true,
                autoplayTimeout: 3000,
                autoplayHoverPause: true,
                margin: 30,
                responsive: {
                    0: {
                        items: 1
                    },
                    768: {
                        items: 2
                    },
                    1400: {
                        items: 3
                    }
                }
            });
            break;
        case 'MasonryGallery':
            var $h1heading = $(blockId + ' h1.block-heading');
            $h1heading.html(objModel.HeadingText);
            $h1heading.css('font-size', objModel.HeadingFontSize + 'px');
            $h1heading.css('color', objModel.HeadingColour);
            var $h1headingAccentColour = $(blockId + ' h1.block-heading span');
            $h1headingAccentColour.css('color', objModel.HeadingAccentColour);
            var $content = $(blockId + ' p.block-content');
            $content.html(objModel.SubHeadingText);
            $content.css('font-size', objModel.SubHeadingFontSize + 'px');
            $content.css('color', objModel.SubHeadingColour);
            var $gallerylist = $(blockId + ' div.masonry');
            $gallerylist.empty();
            $.each(objModel.GalleryItems, function (index, value) {
                var _blockfeatureHtml = '<div class="masonitem">';
                _blockfeatureHtml += '<a href="' + getUrlImage(value.FeaturedImage) + '" class="image-pop" data-fancybox="gallery" title="' + value.Title + '"><img src="' + getUrlImage(value.FeaturedImage)+'" class="img-responsive"></a>';
                _blockfeatureHtml += '</div>';
                $gallerylist.append(_blockfeatureHtml);
            });
            $gallerylist.imagesLoaded(function () {
                $gallerylist.css('height', '');
                $gallerylist.masonry('destroy');
                $gallerylist.removeData('masonry'); // This line to remove masonry's data
                $gallerylist.masonry({
                    columnWidth: 420,
                    isFitWidth: true,
                    itemSelector: '.masonitem'
                });
            });
            break;
        default:
            break;
    }
}
function removeBlock(blockId) {
    var check = confirm('Are you sure you want to delete this block? This action cannot be undone');
    if (check == true) {
        $('#btnDiscardChange').removeAttr('disabled');
        var type = $('#'+blockId).data('type');
        switch (type) {
            case 'HeroPersonal':
                _pageBuilder.BlockHeroes = $.grep(_pageBuilder.BlockHeroes, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'TextAndImage':
                _pageBuilder.BlockTextImages = $.grep(_pageBuilder.BlockTextImages, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'Gallery':
                _pageBuilder.BlockGalleries = $.grep(_pageBuilder.BlockGalleries, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'MasonryGallery':
                _pageBuilder.BlockMasonryGalleries = $.grep(_pageBuilder.BlockMasonryGalleries, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            default:
                break
        }
        blockId = '#' + blockId;
        $(blockId).remove();
        updateDisplayOrderBlocks();
    }
}
function moveUpDown(blockId, isMoveUp) {
    var $blocks = $('#page_templates_builder .profile-pageblock');
    var len = $('#page_templates_builder .profile-pageblock').length;
    var currentIndex = $('#' + blockId).index() - 1;
    if (isMoveUp && currentIndex > 0) {
        jQuery($blocks.eq(currentIndex - 1)).before(jQuery($blocks.eq(currentIndex)));
        currentIndex = currentIndex - 1;
    } else if (!isMoveUp && currentIndex < len) {
        jQuery($blocks.eq(currentIndex + 1)).after($blocks.eq(currentIndex));
        currentIndex = currentIndex + 1;
    }
    updateDisplayOrderBlocks();
}
function moveUpDownItems(itemId, isMoveUp,containerId) {
    var $blocks = $('#' + containerId+' .item-block');
    var len = $blocks.length;
    var currentIndex = $('#' + itemId).index();
    var $currentItem = $blocks.eq(currentIndex);
    if (isMoveUp && currentIndex > 0) {
        $blocks.eq(currentIndex - 1).before($currentItem);
        currentIndex = currentIndex - 1;
    } else if (!isMoveUp && currentIndex < len) {
        $blocks.eq(currentIndex + 1).after($currentItem);
        currentIndex = currentIndex + 1;
    }
    document.getElementById($currentItem.attr('id')).scrollIntoView();
    hideUpDownFirstLastItem(containerId);
}
function updateDisplayOrderBlocks() {
    var $blocks = $('#page_templates_builder .profile-pageblock');
    if ($blocks.length == 0) {
        $('#newstart').show();
        return;
    }
    $.each($blocks, function (index, block) {
        var type = $(block).data('type');
        var blockId = $(block).attr('id');
        switch (type) {
            case 'HeroPersonal':
                var hero = _pageBuilder.BlockHeroes.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (hero)
                    hero.DisplayOrder = (index + 1);
                break;
            case 'TextAndImage':
                var textandimage = _pageBuilder.BlockTextImages.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (textandimage)
                    textandimage.DisplayOrder = (index + 1);
                break;
            case 'Gallery':
                var gallery = _pageBuilder.BlockGalleries.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (gallery)
                    gallery.DisplayOrder = (index + 1);
                break;
            case 'MasonryGallery':
                var masonry = _pageBuilder.BlockMasonryGalleries.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (masonry)
                    masonry.DisplayOrder = (index + 1);
                break;
            default:
                break
        }

    });
    hideUpDownBlock();
}
function hideUpDownBlock() {
    var $blocks = $('#page_templates_builder .profile-pageblock');
    //show ALL
    $blocks.find('button.moveup').show();
    $blocks.find('button.movedown').show();
    //end show ALL
    $blocks.first().find('button.moveup').hide();
    $blocks.last().find('button.movedown').hide();
}
function getBlocksForJumbTo(elm,currentBlockId) {
    var $blocks = $('#page_templates_builder .profile-pageblock');
    if ($blocks.length == 0) {
        return;
    }
    var $select = $(elm);
    $select.empty();
    $select.append("<option value=\"external\">External link</option>");
    $.each($blocks, function (index, block) {
        var type = $(block).data('type');
        var id =$(block).data('id');
        var blockId = $(block).attr('id');
        if (blockId != currentBlockId)
         $select.append("<option value=\"" + (id ? ("#block" + id) : ("#" + blockId)) + "\">Block " + index + " (" + type+")"+"</option>");
    });
    $select.not('.multi-select').select2({ placeholder: "Please select" });
}
function getUrlImage(val) {
    var docapi = $('#api-uri').val();
    if (isUUID(val))
        return docapi + val;
    else
        return val;
}
function isUUID(uuid) {
    let s = "" + uuid;

    s = s.match('^[0-9a-fA-F]{8}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{4}-[0-9a-fA-F]{12}$');
    if (s === null) {
        return false;
    }
    return true;
}
function getPageBuilderById(id) {
    $.getJSON("/ProfilePage/GetJsonUserPageById?id=" + id, function (data) {
        _pageBuilder = data;
        if (!_pageBuilder.BlockHeroes)
            _pageBuilder.BlockHeroes = [];
        if (!_pageBuilder.BlockTextImages)
            _pageBuilder.BlockTextImages = [];
        if (!_pageBuilder.BlockGalleries)
            _pageBuilder.BlockGalleries = [];
        if (!_pageBuilder.BlockMasonryGalleries)
            _pageBuilder.BlockMasonryGalleries = [];
        $('#btnAddBlock').removeAttr('disabled');
    });
}
function addItemsGallary(files) {
    $.each(files, function (index, file) {
        if (file.meta.id) {
            var elmClone = $('#gallery-item-block').clone();
            var $galleryitem = $(elmClone);
            $galleryitem.show();
            var itemID = 'item-' + UniqueId(5);
            $('#gallery-items').append($galleryitem);
            $galleryitem.attr('id', itemID);
            $galleryitem.find('input[name=Title]').val('Gallery item title');
            $galleryitem.find('textarea[name=Content]').val('Image caption content displayed here');
            $galleryitem.find('input[name=FeaturedImage]').val(file.meta.id);
            $galleryitem.find('div.preview').css('background-image', 'url(' + getUrlImage(file.meta.id) + ')');
            $galleryitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'gallery-items\');');
            $galleryitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'gallery-items\')');
            $galleryitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'gallery-items\')');
        }
    });
    if (files.length > 0) {
        $('#gallery-intro').hide();
        $('#gallery-items').show();
    } else {
        $('#gallery-intro').show();
        $('#gallery-items').hide();
    }
    hideUpDownFirstLastItem('gallery-items');
}
function addItemsMasonryGallary(files) {
    $.each(files, function (index, file) {
        if (file.meta.id) {
            var elmClone = $('#gallery-item-block').clone();
            var $galleryitem = $(elmClone);
            $galleryitem.show();
            var itemID = 'item-' + UniqueId(5);
            $('#masonry-items').append($galleryitem);
            $galleryitem.attr('id', itemID);
            $galleryitem.find('input[name=Title]').val('Gallery item title');
            $galleryitem.find('textarea[name=Content]').val('Image caption content displayed here');
            $galleryitem.find('input[name=FeaturedImage]').val(file.meta.id);
            $galleryitem.find('div.preview').css('background-image', 'url(' + getUrlImage(file.meta.id) + ')');
            $galleryitem.find('button.btn-delete').attr('onclick', 'removeGallaryItem(\'#' + itemID + '\');');
        }
    });
    if (files.length > 0) {
        $('#masonry-intro').hide();
        $('#masonry-items').show();
    } else {
        $('#gallery-intro').show();
        $('#masonry-items').hide();
    }
    $('#masonry-items div.item-block').first().find('button.btn-moveup').hide();
    $('#masonry-items div.item-block').last().find('button.btn-movedown').hide();
}
function removeItem(itemID,containerId) {
    $(itemID).remove();
    $('#' + containerId+' div.item-block').first().find('button.btn-moveup').hide();
    $('#' + containerId + ' div.item-block').last().find('button.btn-movedown').hide();
}
function hideUpDownFirstLastItem(containerId) {
    var $items = $('#' + containerId + ' div.item-block');
    //show ALL
    $items.find('button.btn-moveup').show();
    $items.find('button.btn-movedown').show();
    //end show ALL
    $items.first().find('button.btn-moveup').hide();
    $items.last().find('button.btn-movedown').hide();
}
function uploadImageBlock(elm, previewSelector,valueSelector,isdiv) {
    var fileId = $(elm).attr('id');
    var _file = elm.files;
    if (_file && _file.length > 0) {
        $(previewSelector).LoadingOverlay("show", { minSize:50});
        UploadMediaS3ClientSide(fileId).then(function (mediaS3Object) {
            if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
                cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
                return;
            }
            else {
                $(valueSelector).val(mediaS3Object.objectKey);
                if (isdiv) {
                    $(previewSelector).css('background-image', 'url(' + getUrlImage(mediaS3Object.objectKey) + ')');
                    $(previewSelector).css('background-size', 'cover');
                }
                else {
                    $(previewSelector).attr('src','');
                    $(previewSelector).attr('src', getUrlImage(mediaS3Object.objectKey));
                }
                    
            }
            $(previewSelector).LoadingOverlay("hide", true);
        });
    }
}
function uploadNoPreview(fileId, elmIdUri) {
    $('#' + fileId).LoadingOverlay("show");
    UploadMediaS3ClientSide(fileId).then(function (mediaS3Object) {
        if (mediaS3Object.objectKey === "no_image" || mediaS3Object.objectKey === "no-image") {
            cleanBookNotification.error(_L("ERROR_MSG_S3_UPLOAD"), "Qbicles");
            return;
        }
        else {
            $('#' + elmIdUri).val(mediaS3Object.objectKey);
        }
        $('#' + fileId).LoadingOverlay("hide", true);
    });
}
function initPluginPicker(modalId) {
    $(modalId +' .fa-picker').iconpicker();
}
function removeHttps(elm) {
    var val = $(elm).val();
    if (val) {
        var regEx = new RegExp('http://', "ig");
        var regEx1 = new RegExp('https://', "ig");
        $(elm).val(val.replace(regEx, '').replace(regEx1, ''));
    }
}