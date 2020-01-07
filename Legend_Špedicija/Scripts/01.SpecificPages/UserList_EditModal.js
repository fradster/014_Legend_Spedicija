//--------------------------------------------------------
//--Action when Save button is pressed in Edit modal
//-----------------------------------------------------
function Save_click() {
	//check that fields are not empty
	var AllFieldsOK = CheckEditFields();

	if (AllFieldsOK) {
		var idVal = EditInputFields.id.value;
		var loginNameVal = EditInputFields.loginName.value.trim();
		var fNameVal = EditInputFields.fName.value.trim();
		var lNameVal = EditInputFields.lName.value.trim();
		var EmailVal = EditInputFields.Email.value.trim();
		var roleIdVal = EditInputFields.roleId.value;

		UpdateUser(idVal, loginNameVal, fNameVal, lNameVal, EmailVal, roleIdVal);
		$(this).dialog("destroy");
	}
	return AllFieldsOK;
}

//---------------------------------------------------------
//-- Update User, ajax call to UserList method
//-----------------------------------------------------
function UpdateUser(id, logName, fName, lName, Email, roleId) {

	//make UseProfileView object
	var UPV1 = {
		SYSUserID: id,
		IsRoleActive: true,
		LOOKUPRoleID: roleId,
		RoleName: "",
		LoginName: logName,
		FirstName: fName,
		LastName: lName,
		email: Email
	};

	$.ajax ({//call UpdateUserData method in Home controller and transfer parameters with data: command
		//-----------------------------------------------------
		dataType: 'json',
		type: "POST",
		url: "/Home/UserList",
		data: {UPV1},
		success: function (obj1) {
			window.location.href = obj1.url + "?Message=" + obj1.message;
		},
		error: function (xhr, TextStatus, thrownError) {
			window.location.href = "/Home/UserList" + "?Message=" + xhr.status + "." + thrownError;
		}
	});
}

//--------------------------------------------------------
//--chech that fields in Edit dialog are not empty and e-mail is regular
//--mark the missing field and print warning
//--return true if everyrhing is ok
//-----------------------------------------------------
function CheckEditFields() {
	var AllFiledsRegular = false;

	//get input fields values
	var loginNameVal = EditInputFields.loginName.value.trim();
	var fNameVal = EditInputFields.fName.value.trim();
	var lNameVal = EditInputFields.lName.value.trim();
	var EmailVal = EditInputFields.Email.value.trim();
	var roleIdVal = EditInputFields.roleId.value;

	//erase warning fields in modal
	$("#divEdit .alert-warning").empty();

	//checks fields
	if (!loginNameVal) {
		$("#wrn_LoginName").text("Required");
	}
	else if (!fNameVal) {
		$("#wrn_FirstName").text("Required");
	}
	else if (!lNameVal) {
		$("#wrn_LastName").text("Required");
	}
	else if (!EmailVal) {
		$("#wrn_Email").text("Required");
	}
	else if (!CheckMailFormat(EmailVal)) {
		$("#wrn_Email").text("Ivalid mail");
	}
	else if (roleIdVal < 1) {
		$("#wrn_ddlRoles").text("Required");
	}
	else {
		var AllFiledsRegular = true;
	}
	return AllFiledsRegular;
}