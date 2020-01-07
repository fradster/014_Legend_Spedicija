//---------------------------------------------------------
//-- delete user, call Home/DeleteUser action
//-----------------------------------------------------
function DeleteUser(id) {
	$.ajax({
		type: "POST",
		url: "/Home/DeleteUser",
		data: {userID: id},
		success: function (message) {
			window.location.href = "/Home/UserList" + "?Message=" + message;
		},
		error: function (xhr, TextStatus, thrownError) {
			window.location.href = "/Home/UserList" + "?Message=" + xhr.status + "." + thrownError;
		}
	});
}


