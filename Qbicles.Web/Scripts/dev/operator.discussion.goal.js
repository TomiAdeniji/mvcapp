var CountPost = 1, CountMedia = 1, busycomment = false;
function validateAddComment() {
	var message = $('#txt-comment-link').val();
	if (message.length > 1500)
		$('#addcomment-error').show();
	else
		$('#addcomment-error').hide();
}
function AddCommentToSocialPostDiscussion(discussionKey) {
	if (busycomment)
		return;
	var message = $('#txt-comment-link');
	if (message.val() && !$('#addcomment-error').is(':visible')) {
		isPlaceholder(true, '#list-comments-discussion');
		busycomment = true;
		$.ajax({
			url: "/QbicleComments/AddComment2Discussion",
			data: { message: message.val(), disKey: discussionKey },
			type: "POST",
			success: function (result) {
				if (result) {
					message.val("");
				}
				busycomment = false;
			},
			error: function (error) {
				cleanBookNotification.error(_L("ERROR_MSG_EXCEPTION_SYSTEM"), "Sales Marketing");
				isPlaceholder(false, '');
				busycomment = false;
			}
		});
	}
}
function LoadMorePostsDiscussion(activityKey, pageSize, divId) {

	$.ajax({
		url: '/Qbicles/LoadMoreActivityPosts',
		data: {
			activityKey: activityKey,
			size: CountPost * pageSize
		},
		cache: false,
		type: "POST",
		dataType: 'html',
		beforeSend: function (xhr) {
		},
		success: function (response) {
			if (response === "") {
				$('#btnLoadPosts').remove();
				return;
			}
			$('#' + divId).append(response).fadeIn(250);
			CountPost = CountPost + 1;
		},
		error: function (er) {
			
			cleanBookNotification.error(er.responseText, "Qbicles");
		}
	});

}
function LoadMoreMediasDiscussion(activityId, pageSize, divId) {
	$.ajax({
		url: '/Qbicles/LoadMoreActivityMedias',
		data: {
			activityId: activityId,
			size: CountMedia * pageSize
		},
		cache: false,
		type: "POST",
		dataType: 'html',
		beforeSend: function (xhr) {
		},
		success: function (response) {
			if (response === "") {
				$('#btnLoadMedias').remove();
				return;
			}
			$('#' + divId).append(response).fadeIn(250);
			CountMedia = CountMedia + 1;
		},
		error: function (er) {
			cleanBookNotification.error(er.responseText, "Qbicles");
		}
	});
}