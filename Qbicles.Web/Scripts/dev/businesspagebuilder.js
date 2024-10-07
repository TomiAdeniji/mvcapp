// Declare a global object to store view data.
var _pageBuilder;

_pageBuilder = {
    Id: 0,
    PageTitle: '',
    Status: 'IsDraft',
    BlockHeroes: [],
    BlockPromotions: [],
    BlockTestimonials: [],
    BlockTextImages: [],
    BlockGalleries: [],
    BlockFeatures: []
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
    $('#page_templates_builder .nvtestimonialsowl').owlCarousel({
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
            600: {
                items: 2
            },
            1024: {
                items: 3
            }
        }
    });
}
//end Data-Bind
function saveBusinessPageBuilder(status) {
    _pageBuilder.Status = status;
    if ($('#frmpageBuilder').valid()) {
        $.LoadingOverlay("show");
        $.each(_pageBuilder.BlockHeroes, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockFeatures, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockTextImages, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockPromotions, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockGalleries, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.each(_pageBuilder.BlockTestimonials, function (index, block) {
            if (block.Id > 0)
                block.ElmId = '';
        });
        $.ajax({
            type: 'POST',
            url: '/ProfilePage/SaveBusinessPageBuilder',
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
        $('#' + idCloneBlock + ' .nvtestimonialsowl').owlCarousel({
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
                600: {
                    items: 2
                },
                1024: {
                    items: 3
                }
            }
        });
        initDefaultBlockData(type, idCloneBlock);
        hideUpDownBlock();
    }
}
function initDefaultBlockData(type, elmid) {
    switch (type) {
        case 'Hero':
            var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
            var hero = {
                ElmId: elmid,
                Id: 0,
                HeroBackgroundType: 'Gradient',
                HeroBackgroundColour: '#60a7bd',
                HeroGradientColour1: '#1dd8cf',
                HeroGradientColour2: '#8845f0',
                HeroBackGroundImage: '',
                HeroHeadingText: 'Promotional <span>hero</span>',
                HeroHeadingFontSize: 38,
                HeroHeadingColour: '#ffffff',
                HeroHeadingAccentColour: '#d0d265',
                HeroSubHeadingText: 'With a strapline included',
                HeroSubHeadingFontSize: 20,
                HeroSubHeadingColour: '#ffffff',
                HeroFeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/nvfeature.png',
                HeroLogo: '/Content/DesignStyle/img/pagebuilder/demos/mslogo.png',
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'Hero',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockHeroes.push(hero);
            break;
        case 'Features':
            var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
            var feature = {
                ElmId: elmid,
                Id: 0,
                HeadingText: 'Subsection heading',
                HeadingFontSize: 32,
                HeadingColour: '#224157',
                FeatureHeadingAccentColour:'#d0d265',
                SubHeadingText: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Fusce sollicitudin ante et ante pellentesque, at ultrices ipsum aliquet. Vestibulum semper et justo sed auctor.',
                SubHeadingFontSize: 16,
                SubHeadingColour: '#657581',
                FeatureItems: [{
                    Id: 0,
                    FeatureTitle: 'Feature one',
                    FeatureContent: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin blandit tempor purus. Sed mollis, odio eu maximus iaculis, massa nunc porta orci, in porta erat leo quis risus. Etiam condimentum dolor a dolor rhoncus iaculis.',
                    FeatureIcon: 'fa fa-chalkboard',
                    FeatureColour: '#333333',
                    DisplayOrder: 1
                }],
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'FeatureList',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockFeatures.push(feature);
            break;
        case 'TextAndImage':
            var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
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
        case 'Promo':
            var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
            var promo = {
                ElmId: elmid,
                Id: 0,
                HeadingText: 'Big feature or promotion here',
                HeadingFontSize: 46,
                HeadingColour: '#224157',
                HeadingAccentColour: '#d0d265',
                SubHeadingText: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Nulla malesuada dolor ut arcu maximus, sit amet pretium neque scelerisque. Curabitur vel diam arcu. Aliquam egestas sem ut tincidunt porttitor. Quisque porta massa sed porttitor rhoncus.',
                SubHeadingFontSize: 16,
                SubHeadingColour: '#657581',
                IsIncludeButton: true,
                ButtonText: 'Button example',
                ButtonColour: '#2990ea',
                ButtonJumpTo: 0,
                ExternalLink: '',
                FeaturedImage: '/Content/DesignStyle/img/pagebuilder/demos/promo-ex.jpg',
                Items: [{
                    Id: 0,
                    Icon: 'fa fa-check',
                    Colour: '#5cb85c',
                    Text: 'Some descriptive text can be added here',
                    DisplayOrder: 1
                },
                {
                    Id: 0,
                    Icon: 'fa fa-check',
                    Colour: '#5cb85c',
                    Text: 'Some descriptive text can be added here',
                    DisplayOrder: 2
                },
                {
                    Id: 0,
                    Icon: 'fa fa-check',
                    Colour: '#5cb85c',
                    Text: 'Some descriptive text can be added here',
                    DisplayOrder: 3
                }],
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'Promotion',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockPromotions.push(promo);
            break;
        case 'Gallery':
            var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
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
        case 'Testimonial':
            var currentDisplayOrder = $('#page_templates_builder .profile-pageblock').length;
            var testimonial = {
                ElmId: elmid,
                Id: 0,
                TestimonialItems: [{
                    Id: 0,
                    PersonName: 'Sarah Price',
                    FurtherInfo: 'Graphic designer',
                    Content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin blandit tempor purus. Sed mollis, odio eu maximus iaculis, massa nunc porta orci, in porta erat leo quis risus. Etiam condimentum dolor a dolor rhoncus iaculis.',
                    AvatarUri: '/Content/DesignStyle/img/pagebuilder/demos/av_1.jpg',
                    DisplayOrder: 1
                },
                {
                    Id: 0,
                    PersonName: 'Richard McAllister',
                    FurtherInfo: 'UX consultant',
                    Content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin blandit tempor purus. Sed mollis, odio eu maximus iaculis, massa nunc porta orci, in porta erat leo quis risus. Etiam condimentum dolor a dolor rhoncus iaculis.',
                    AvatarUri: '/Content/DesignStyle/img/pagebuilder/demos/av_2.jpg',
                    DisplayOrder: 2
                },
                {
                    Id: 0,
                    PersonName: 'Peter Jones',
                    FurtherInfo: 'Web content admin',
                    Content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin blandit tempor purus. Sed mollis, odio eu maximus iaculis, massa nunc porta orci, in porta erat leo quis risus. Etiam condimentum dolor a dolor rhoncus iaculis.',
                    AvatarUri: '/Content/DesignStyle/img/pagebuilder/demos/av_3.jpg',
                    DisplayOrder: 3
                }],
                DisplayOrder: (currentDisplayOrder == 0 ? 1 : (currentDisplayOrder + 1)),
                Type: 'Testimonial',
                ParentPage: { Id: _pageBuilder.Id }
            };
            _pageBuilder.BlockTestimonials.push(testimonial);
            break;
        default:
            break
    }
}
function bindDataForBlock(blockId, type) {
    switch (type) {
        case 'Hero':
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
            $(modalId + ' input[name=HeroLogo]').val(hero.HeroLogo);
            $(modalId + ' .preview-HeroLogo').attr('src', getUrlImage(hero.HeroLogo));
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            $('#drag-drop-area').data('blockid', blockId);
            $(modalId).modal('show');
            break;
        case 'Features':
            var modalId = '#pages-blocks-features-edit';
            var feature = _pageBuilder.BlockFeatures.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (feature && !feature.ElmId)
                feature.ElmId = ('block' + feature.Id);
            else if (!feature)
                return;
            $(modalId + ' input[name=HeadingText]').val(feature.HeadingText);
            $(modalId + ' input[name=HeadingFontSize]').val(feature.HeadingFontSize);
            $(modalId + ' input[name=HeadingColour]').val(feature.HeadingColour);
            $(modalId + ' input[name=FeatureHeadingAccentColour]').val(feature.FeatureHeadingAccentColour);
            $(modalId + ' input[name=SubHeadingText]').val(feature.SubHeadingText);
            $(modalId + ' input[name=SubHeadingFontSize]').val(feature.SubHeadingFontSize);
            $(modalId + ' input[name=SubHeadingColour]').val(feature.SubHeadingColour);
            $('#features-Items').empty();
            $.each(feature.FeatureItems, function (index, value) {
                var elmClone = $('#feaute-item-block').clone()
                var $feauteitem = $(elmClone);
                $feauteitem.show();
                var itemID = 'item-' + UniqueId(5);
                $('#features-Items').append($feauteitem);
                $feauteitem.attr('id', itemID);
                $feauteitem.find('input[name=FeatureTitle]').val(value.FeatureTitle);
                $feauteitem.find('input[name=FeatureIcon]').val(value.FeatureIcon);
                $feauteitem.find('input[name=FeatureColour]').val(value.FeatureColour);
                $feauteitem.find('textarea[name=FeatureContent]').val(value.FeatureContent);
                $feauteitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'features-Items\');');
                $feauteitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'features-Items\')');
                $feauteitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'features-Items\')');
            });
            hideUpDownFirstLastItem('features-Items');
            $(modalId + ' button.btn-addfeature-item').attr('onclick', 'addItemFeature()');
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            initPluginPicker(modalId);
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
            if (textAndImage.IsIncludeButton)
                $('.btn-options-30').show();
            else
                $('.btn-options-30').hide();
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
        case 'Promo':
            var modalId = '#pages-blocks-promo-edit';
            var promo = _pageBuilder.BlockPromotions.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (promo && !promo.ElmId)
                promo.ElmId = ('block' + promo.Id);
            else if (!promo)
                return;
            $(modalId + ' input[name=HeadingAccentColour]').val(promo.HeadingAccentColour);
            $(modalId + ' input[name=HeadingText]').val(promo.HeadingText);
            $(modalId + ' input[name=HeadingFontSize]').val(promo.HeadingFontSize);
            $(modalId + ' input[name=HeadingColour]').val(promo.HeadingColour);
            $(modalId + ' input[name=SubHeadingText]').val(promo.SubHeadingText);
            $(modalId + ' input[name=SubHeadingFontSize]').val(promo.SubHeadingFontSize);
            $(modalId + ' input[name=SubHeadingColour]').val(promo.SubHeadingColour);
            $(modalId + ' input[name=IsIncludeButton]').prop('checked', promo.IsIncludeButton);
            if (promo.IsIncludeButton)
                $('.btn-options-50').show();
            else
                $('.btn-options-50').hide();
            $(modalId + ' input[name=ButtonText]').val(promo.ButtonText);
            $(modalId + ' input[name=ButtonColour]').val(promo.ButtonColour);
            getBlocksForJumbTo(modalId + ' select[name=ButtonJumpTo]', blockId);
            $(modalId + ' select[name=ButtonJumpTo]').val((promo.ButtonJumpTo == '0' ? 'external' : promo.ButtonJumpTo)).change();
            $(modalId + ' input[name=ExternalLink]').val(promo.ExternalLink);
            $(modalId + ' input[name=FeaturedImage]').val(promo.FeaturedImage);
            $(modalId + ' .preview-FeaturedImage').attr('src', getUrlImage(promo.FeaturedImage));
            $('#promo-items').empty();
            $.each(promo.Items, function (index, value) {
                var elmClone = $('#promo-item-block').clone()
                var $promoitem = $(elmClone);
                $promoitem.show();
                var itemID = 'item-' + UniqueId(5);
                $('#promo-items').append($promoitem);
                $promoitem.attr('id', itemID);
                $promoitem.find('input[name=Icon]').val(value.Icon);
                $promoitem.find('input[name=Colour]').val(value.Colour);
                $promoitem.find('input[name=Text]').val(value.Text);
                $promoitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'promo-items\');');
                $promoitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'promo-items\')');
                $promoitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'promo-items\')');
            });
            hideUpDownFirstLastItem('promo-items');
            $(modalId + ' button.btn-update-block').attr('onclick', 'updateDataBlock(\'' + blockId + '\',\'' + type + '\')');
            initPluginPicker(modalId);
            $(modalId).modal('show');
            break;
        case 'Testimonial':
            var modalId = '#pages-blocks-testimonials-edit';
            var testimonial = _pageBuilder.BlockTestimonials.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
            if (testimonial && !testimonial.ElmId)
                testimonial.ElmId = ('block' + testimonial.Id);
            else if (!testimonial)
                return;
            $('#testimonial-items').empty();
            $.each(testimonial.TestimonialItems, function (index, value) {
                var elmClone = $('#testimonial-item-block').clone()
                var $testimonialitem = $(elmClone);
                $testimonialitem.show();
                var itemID = 'item-' + UniqueId(5);
                $('#testimonial-items').append($testimonialitem);
                $testimonialitem.attr('id', itemID);
                $testimonialitem.find('div.nvavatar').css('background-image', 'url(' + getUrlImage(value.AvatarUri) + ')');
                $testimonialitem.find('input[name=AvatarUri]').val(value.AvatarUri);
                $testimonialitem.find('input[name=PersonName]').val(value.PersonName);
                $testimonialitem.find('input[name=FurtherInfo]').val(value.FurtherInfo);
                $testimonialitem.find('textarea[name=Content]').val(value.Content);
                $testimonialitem.find('button.btn-changeimage').attr('onclick', "$('#file" + itemID + "').trigger('click');");
                var $avatarfile=$testimonialitem.find('input[name=Avatar-File]');
                $avatarfile.attr('id', 'file' + itemID);
                $avatarfile.attr('onchange', "uploadImageBlock(this,'#" + itemID + " .nvavatar','#" + itemID + "  input[name=AvatarUri]',true)");
                $testimonialitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'testimonial-items\');');
                $testimonialitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'testimonial-items\')');
                $testimonialitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'testimonial-items\')');
            });
            hideUpDownFirstLastItem('testimonial-items');
            $(modalId + ' button.btn-addtestimonial-item').attr('onclick', 'addItemTestimonial()');
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
        default:
            break;
    }
}
function updateDataBlock(blockId, type) {
    $('#btnDiscardChange').removeAttr('disabled');
    switch (type) {
        case 'Hero':
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
            hero.HeroLogo = $(modalId + ' input[name=HeroLogo]').val();
            updateBlockPreview(blockId, hero, type);
            $(modalId).modal('hide');
            break;
        case 'Features':
            var modalId = '#pages-blocks-features-edit';
            var feature = _pageBuilder.BlockFeatures.find(x => x.ElmId == blockId);
            feature.HeadingText = $(modalId + ' input[name=HeadingText]').val();
            feature.HeadingFontSize = $(modalId + ' input[name=HeadingFontSize]').val();
            feature.HeadingColour = $(modalId + ' input[name=HeadingColour]').val();
            feature.FeatureHeadingAccentColour = $(modalId + ' input[name=FeatureHeadingAccentColour]').val();
            feature.SubHeadingText = $(modalId + ' textarea[name=SubHeadingText]').val();
            feature.SubHeadingFontSize = $(modalId + ' input[name=SubHeadingFontSize]').val();
            feature.SubHeadingColour = $(modalId + ' input[name=SubHeadingColour]').val();
            var $items = $('#features-Items div.item-block');
            feature.FeatureItems = [];
            $items.each(function (index, element) {
                // element == this
                var $feauteitem = $(element);
                var _featureTitle = $feauteitem.find('input[name=FeatureTitle]').val();
                var _featureContent = $feauteitem.find('textarea[name=FeatureContent]').val();
                if (_featureTitle && _featureContent) {
                    var data = {
                        Id: 0,
                        FeatureTitle: _featureTitle,
                        FeatureContent: _featureContent,
                        FeatureIcon: $feauteitem.find('input[name=FeatureIcon]').val(),
                        FeatureColour: $feauteitem.find('input[name=FeatureColour]').val(),
                        DisplayOrder: (index + 1)
                    };
                    feature.FeatureItems.push(data);
                }
            });
            updateBlockPreview(blockId, feature, type);
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
        case 'Promo':
            var modalId = '#pages-blocks-promo-edit';
            var promo = _pageBuilder.BlockPromotions.find(x => x.ElmId == blockId);
            promo.HeadingAccentColour = $(modalId + ' input[name=HeadingAccentColour]').val();
            promo.HeadingText = $(modalId + ' input[name=HeadingText]').val();
            promo.HeadingFontSize = $(modalId + ' input[name=HeadingFontSize]').val();
            promo.HeadingColour = $(modalId + ' input[name=HeadingColour]').val();
            promo.SubHeadingText = $(modalId + ' textarea[name=SubHeadingText]').val();
            promo.SubHeadingFontSize = $(modalId + ' input[name=SubHeadingFontSize]').val();
            promo.SubHeadingColour = $(modalId + ' input[name=SubHeadingColour]').val();
            promo.IsIncludeButton = $(modalId + ' input[name=IsIncludeButton]').prop('checked');
            promo.ButtonText = $(modalId + ' input[name=ButtonText]').val();
            promo.ButtonColour = $(modalId + ' input[name=ButtonColour]').val();
            var btnjumptoval = $(modalId + ' select[name=ButtonJumpTo]').val();
            promo.ButtonJumpTo = (btnjumptoval == 'external' ? 0 : btnjumptoval);
            promo.ExternalLink = $(modalId + ' input[name=ExternalLink]').val();
            promo.FeaturedImage = $(modalId + ' input[name=FeaturedImage]').val();
            var $items = $('#promo-items div.item-block');
            promo.Items = [];
            $items.each(function (index, element) {
                var $promoitem = $(element);
                var data = {
                    Id: 0,
                    Icon: $promoitem.find('input[name=Icon]').val(),
                    Colour: $promoitem.find('input[name=Colour]').val(),
                    Text: $promoitem.find('input[name=Text]').val(),
                    DisplayOrder: (index + 1)
                };
                promo.Items.push(data);
            });
            updateBlockPreview(blockId, promo, type);
            $(modalId).modal('hide');
            break;
        case 'Testimonial':
            var modalId = '#pages-blocks-testimonials-edit';
            var testimonial = _pageBuilder.BlockTestimonials.find(x => x.ElmId == blockId);
            var $items = $('#testimonial-items div.item-block');
            testimonial.TestimonialItems = [];
            $items.each(function (index, element) {
                var $testimonialitem = $(element);
                var data = {
                    Id: 0,
                    PersonName: $testimonialitem.find('input[name=PersonName]').val(),
                    FurtherInfo: $testimonialitem.find('input[name=FurtherInfo]').val(),
                    Content: $testimonialitem.find('textarea[name=Content]').val(),
                    AvatarUri: $testimonialitem.find('input[name=AvatarUri]').val(),
                    DisplayOrder: (index + 1)
                };
                testimonial.TestimonialItems.push(data);
            });
            updateBlockPreview(blockId, testimonial, type);
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
        default:
            break;
    }
}
function updateBlockPreview(blockId, objModel, type) {
    var blockId = '#' + blockId;
    switch (type) {
        case 'Hero':
            if (objModel.HeroBackgroundType == 'Gradient') {
                $(blockId).css('background', 'linear-gradient(140deg, ' + objModel.HeroGradientColour1 + ' 0%, ' + objModel.HeroGradientColour2 + ' 100%)');
            } else if (objModel.HeroBackgroundType == 'SingleColour') {
                $(blockId).css('background','none');
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
            $(blockId + ' img.nvlogo').attr('src', getUrlImage(objModel.HeroLogo));
            $(blockId + ' img.nvfeature').attr('src', getUrlImage(objModel.HeroFeaturedImage));
            break;
        case 'Features':
            var $h1heading = $(blockId + ' h1.block-heading');
            $h1heading.html(objModel.HeadingText);
            $h1heading.css('font-size', objModel.HeadingFontSize + 'px');
            $h1heading.css('color', objModel.HeadingColour);
            var $h1headingAccentColour = $(blockId + ' h1.block-heading span');
            $h1headingAccentColour.css('color', objModel.FeatureHeadingAccentColour);
            var $h5heading = $(blockId + ' h5.block-subheading');
            $h5heading.html(objModel.SubHeadingText);
            $h5heading.css('font-size', objModel.SubHeadingFontSize + 'px');
            $h5heading.css('color', objModel.SubHeadingColour);
            var $nvfeaturelist = $(blockId + ' div.nvfeaturelist');
            $nvfeaturelist.empty();
            $.each(objModel.FeatureItems, function (index, value) {
                var _blockfeatureHtml = '<div class="col">';
                _blockfeatureHtml += '<div class="nvicon" style="color:' + value.FeatureColour + '"><i class="' + value.FeatureIcon + '"></i></div>';
                _blockfeatureHtml += '<div class="nvfeaturecontent">';
                _blockfeatureHtml += '<h6 class="block-feature-heading">' + value.FeatureTitle + '</h6>';
                _blockfeatureHtml += '<p class="block-feature-content">' + value.FeatureContent + '</p>';
                _blockfeatureHtml += '</div>';
                _blockfeatureHtml += '</div>';
                $nvfeaturelist.append(_blockfeatureHtml);
            });
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
        case 'Promo':
            var $h1heading = $(blockId + ' h1.block-heading');
            $h1heading.html(objModel.HeadingText);
            $h1heading.css('font-size', objModel.HeadingFontSize + 'px');
            $h1heading.css('color', objModel.HeadingColour);
            var $h1headingAccentColour = $(blockId + ' h1.block-heading span');
            $h1headingAccentColour.css('color', objModel.HeadingAccentColour);
            var $h3heading = $(blockId + ' h3.block-subheading');
            $h3heading.html(objModel.SubHeadingText);
            $h3heading.css('font-size', objModel.SubHeadingFontSize + 'px');
            $h3heading.css('color', objModel.SubHeadingColour);
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
            $(blockId + ' img.promo-img').attr('src', getUrlImage(objModel.FeaturedImage));
            var $nvfeaturelist = $(blockId + ' ul.promo-features');
            $nvfeaturelist.empty();
            $.each(objModel.Items, function (index, value) {
                var _blockfeatureHtml = '<li>';
                _blockfeatureHtml += '<i class="' + value.Icon + '" style="color:' + value.Colour + '"></i> ';
                _blockfeatureHtml += '<span class="promo-list-item">' + value.Text + '</span>';
                _blockfeatureHtml += '</li>';
                $nvfeaturelist.append(_blockfeatureHtml);
            });
            break;
        case 'Testimonial':
            var $nvtestimoniallist = $(blockId + ' div.nvtestimonialsowl');
            $nvtestimoniallist.empty();
            $.each(objModel.TestimonialItems, function (index, value) {
                var _blockfeatureHtml = '<div class="item"><div class="nvtestimonial">';
                _blockfeatureHtml += '<div class="nvavatar" style="background-image: url(\'' + getUrlImage(value.AvatarUri) + '\');">&nbsp;</div>';
                _blockfeatureHtml += ' <h2 class="testimonial-name">' + value.PersonName + '</h2>';
                _blockfeatureHtml += '<h3 class="testimonial-info">' + value.FurtherInfo + '</h3>';
                _blockfeatureHtml += '<p class="testimonial-content">"' + value.Content + '"</p>';
                _blockfeatureHtml += '</div></div>';
                $nvtestimoniallist.append(_blockfeatureHtml);
            });
            $nvtestimoniallist.owlCarousel('destroy');
            $nvtestimoniallist.owlCarousel({
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
                    600: {
                        items: 2
                    },
                    1024: {
                        items: 3
                    }
                }
            });
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
        default:
            break;
    }
}
function removeBlock(blockId) {
    var check = confirm('Are you sure you want to delete this block?');
    if (check == true) {
        $('#btnDiscardChange').removeAttr('disabled');
        var type = $('#'+blockId).data('type');
        switch (type) {
            case 'Hero':
                _pageBuilder.BlockHeroes = $.grep(_pageBuilder.BlockHeroes, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'Features':
                _pageBuilder.BlockFeatures = $.grep(_pageBuilder.BlockFeatures, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'TextAndImage':
                _pageBuilder.BlockTextImages = $.grep(_pageBuilder.BlockTextImages, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'Promo':
                _pageBuilder.BlockPromotions = $.grep(_pageBuilder.BlockPromotions, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'Gallery':
                _pageBuilder.BlockGalleries = $.grep(_pageBuilder.BlockGalleries, function (value) {
                    return value.ElmId != blockId && blockId != ('block' + value.Id);
                });
                break;
            case 'Testimonial':
                _pageBuilder.BlockTestimonials = $.grep(_pageBuilder.BlockTestimonials, function (value) {
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
            case 'Hero':
                var hero = _pageBuilder.BlockHeroes.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (hero)
                    hero.DisplayOrder = (index + 1);
                break;
            case 'Features':
                var feature = _pageBuilder.BlockFeatures.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (feature)
                    feature.DisplayOrder = (index + 1);
                break;
            case 'TextAndImage':
                var textandimage = _pageBuilder.BlockTextImages.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (textandimage)
                    textandimage.DisplayOrder = (index + 1);
                break;
            case 'Promo':
                var promo = _pageBuilder.BlockPromotions.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (promo)
                    promo.DisplayOrder = (index + 1);
                break;
            case 'Gallery':
                var gallery = _pageBuilder.BlockGalleries.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (gallery)
                    gallery.DisplayOrder = (index + 1);
                break;
            case 'Testimonial':
                var testimonial = _pageBuilder.BlockTestimonials.find(x => x.ElmId == blockId || blockId == ('block' + x.Id));
                if (testimonial)
                    testimonial.DisplayOrder = (index + 1);
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
function getBlocksForJumbTo(elm, currentBlockId) {
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
    $.getJSON("/ProfilePage/GetJsonBusinessPageById?id=" + id, function (data) {
        _pageBuilder = data;
        if (!_pageBuilder.BlockHeroes)
            _pageBuilder.BlockHeroes = [];
        if (!_pageBuilder.BlockPromotions)
            _pageBuilder.BlockPromotions = [];
        if (!_pageBuilder.BlockTestimonials)
            _pageBuilder.BlockTestimonials = [];
        if (!_pageBuilder.BlockTextImages)
            _pageBuilder.BlockTextImages = [];
        if (!_pageBuilder.BlockGalleries)
            _pageBuilder.BlockGalleries = [];
        if (!_pageBuilder.BlockFeatures)
            _pageBuilder.BlockFeatures = [];
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
function removeItem(itemID,containerId) {
    $(itemID).remove();
    $('#' + containerId+' div.item-block').first().find('button.btn-moveup').hide();
    $('#' + containerId + ' div.item-block').last().find('button.btn-movedown').hide();
}
function addItemFeature() {
    var item = {
        Id: 0,
        FeatureTitle: '',
        FeatureContent: '',
        FeatureIcon: 'fa fa-chalkboard',
        FeatureColour: '#333333'
    };
    var elmClone = $('#feaute-item-block').clone()
    var $feauteitem = $(elmClone);
    $feauteitem.show();
    var itemID = 'item-' + UniqueId(5);
    $('#features-Items').append($feauteitem);
    $feauteitem.attr('id', itemID);
    $feauteitem.find('input[name=FeatureTitle]').val(item.FeatureTitle);
    $feauteitem.find('input[name=FeatureIcon]').val(item.FeatureIcon);
    $feauteitem.find('input[name=FeatureColour]').val(item.FeatureColour);
    $feauteitem.find('textarea[name=FeatureContent]').val(item.FeatureContent);
    $feauteitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'features-Items\');');
    $feauteitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'features-Items\')');
    $feauteitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'features-Items\')');
    hideUpDownFirstLastItem('features-Items');
    initPluginPicker('#features-Items');
}
function addItemTestimonial() {
    var item = {
        Id: 0,
        PersonName: 'Sarah Price',
        FurtherInfo: 'Graphic designer',
        Content: 'Lorem ipsum dolor sit amet, consectetur adipiscing elit. Proin blandit tempor purus. Sed mollis, odio eu maximus iaculis, massa nunc porta orci, in porta erat leo quis risus. Etiam condimentum dolor a dolor rhoncus iaculis.',
        AvatarUri: '/Content/DesignStyle/img/pagebuilder/demos/av_1.jpg',
        DisplayOrder: 1
    };
    var elmClone = $('#testimonial-item-block').clone()
    var $testimonialitem = $(elmClone);
    $testimonialitem.show();
    var itemID = 'item-' + UniqueId(5);
    $('#testimonial-items').append($testimonialitem);
    $testimonialitem.attr('id', itemID);
    $testimonialitem.find('div.nvavatar').css('background-image', 'url(' + getUrlImage(item.AvatarUri) + ')');
    $testimonialitem.find('input[name=AvatarUri]').val(item.AvatarUri);
    $testimonialitem.find('input[name=PersonName]').val(item.PersonName);
    $testimonialitem.find('input[name=FurtherInfo]').val(item.FurtherInfo);
    $testimonialitem.find('textarea[name=Content]').val(item.Content);
    $testimonialitem.find('button.btn-changeimage').attr('onclick', "$('#file" + itemID + "').trigger('click');");
    var $avatarfile = $testimonialitem.find('input[name=Avatar-File]');
    $avatarfile.attr('id', 'file' + itemID);
    $avatarfile.attr('onchange', "uploadImageBlock(this,'#" + itemID + " .nvavatar','#" + itemID + "  input[name=AvatarUri]',true)");
    $testimonialitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'testimonial-items\');');
    $testimonialitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'testimonial-items\')');
    $testimonialitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'testimonial-items\')');
    hideUpDownFirstLastItem('testimonial-items');
}
function addItemPromo() {
    var item = {
        Id: 0,
        Icon: 'fa fa-check',
        Colour: '#5cb85c',
        Text: 'Some descriptive text can be added here',
        DisplayOrder: 1
    };
    var elmClone = $('#promo-item-block').clone()
    var $promoitem = $(elmClone);
    $promoitem.show();
    var itemID = 'item-' + UniqueId(5);
    $('#promo-items').append($promoitem);
    $promoitem.attr('id', itemID);
    $promoitem.find('input[name=Icon]').val(item.Icon);
    $promoitem.find('input[name=Colour]').val(item.Colour);
    $promoitem.find('input[name=Text]').val(item.Text);
    $promoitem.find('button.btn-delete').attr('onclick', 'removeItem(\'#' + itemID + '\',\'promo-items\');');
    $promoitem.find('button.btn-moveup').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',true,\'promo-items\')');
    $promoitem.find('button.btn-movedown').attr('onclick', 'moveUpDownItems(\'' + itemID + '\',false,\'promo-items\')');
    hideUpDownFirstLastItem('promo-items');
    initPluginPicker('#promo-items');
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