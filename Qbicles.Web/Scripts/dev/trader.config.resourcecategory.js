// Document
function getDataReCategoriesDocument() {
	var ajaxUri = '/TraderConfiguration/GetResouceCategoryDocumentData';
	$('#document_category_document_table').LoadingOverlay("show", { minSize: "70x60px" });
	$('#document_category_document_table').empty();
	$('#document_category_document_table').load(ajaxUri, function () {
		$('#document_category_document_table').LoadingOverlay("hide");
	});
}
// Image
function getDataReCategoriesImage() {
	var ajaxUri = '/TraderConfiguration/GetResouceCategoryImageData';
	$('#resource_category_image_table').LoadingOverlay("show", { minSize: "70x60px" });
	$('#resource_category_image_table').empty();
	$('#resource_category_image_table').load(ajaxUri, function () {
		$('#resource_category_image_table').LoadingOverlay("hide");
	});
}
// save Category
function saveReCategory(id, type, name) {
	var category = {
		Id: id,
		Type: type,
		Name: $('#resource_category_' + id).val()
	}
	if (name)
		category.Name = name;
	LoadingOverlay();
	$.ajax({
		type: 'post',
		url: '/TraderConfiguration/UpdateResourceCategory',
		data: { category: category },
		dataType: 'json',
		success: function (response) {
			$.LoadingOverlay("hide");
			if (response.actionVal === 1) {
				cleanBookNotification.createSuccess();
				getDataReCategoriesDocument();
				getDataReCategoriesImage();
			} else if (response.actionVal === 2) {
				cleanBookNotification.updateSuccess();
				getDataReCategoriesDocument();
				getDataReCategoriesImage();
			} else if (response.actionVal === 3) {
				cleanBookNotification.error(response.msg, "Qbicles");
				return;
			}
			if (response.result && response.actionVal === 1)
				if (type == 'Document') {
					$('.newcat-doc').toggle();
					$('.addcat-doc').toggle();
				} else {
					$('.newcat-img').toggle();
					$('.addcat-img').toggle();
				}
		},
		error: function (er) {
			$.LoadingOverlay("hide");
			cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Qbicles");
		}
	});
}
function deleteReCagegory(id, type) {
	var result = confirm("Do you want delete an item : '" + $('#resource_category_' + id).val() + "'?");
	if (result == false) {
		return;
	}
	var url = "/TraderConfiguration/DeleteResourceCategory?id=" + id;
	$.LoadingOverlay("show");
	$.ajax({
		url: url,
		type: "delete",
		dataType: "json",
		success: function (rs) {
			LoadingOverlayEnd();
			if (rs.actionVal == 1) {
				cleanBookNotification.removeSuccess();
				if (type === 'Document') {
					getDataReCategoriesDocument();
				} else {
					getDataReCategoriesImage();
				}
			} else if (rs.actionVal == 3) {
				cleanBookNotification.removeFail();
			}
		},
		error: function (err) {
			cleanBookNotification.error(err, "Qbicles");
			LoadingOverlayEnd();
		}
	}).always(function () {
		LoadingOverlayEnd();
	});
}