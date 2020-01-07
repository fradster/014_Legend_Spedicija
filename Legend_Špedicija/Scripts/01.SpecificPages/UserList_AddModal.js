//---------------------------------------------------------
//--read Add USer dialog fields, send data to
//-----------------------------------------------------
function AddUser() {
	//make input field object
	var AddInputFields = {
		loginName: document.querySelector('#add_LoginName'),
		fName: document.querySelector("#add_FirstName"),
		lName: document.querySelector("#add_LastName"),
		Email: document.querySelector("#add_Email"),
		//roleId: document.querySelector("#add_ddlRoles")
	}
	//check input fields
	var AllAddFieldsOk = CheckAddFields(AddInputFields);

	if (AllAddFieldsOk) {//if all fields are ok
		//make UserSignUpView model object
		var USUV1 = {
			//LOOKUPRoleID: AddInputFields["roleId"].value.trim(),
			LoginName: AddInputFields.loginName.value.trim(),
			FirstName: AddInputFields.fName.value.trim(),
			LastName: AddInputFields.lName.value.trim(),
			email: AddInputFields.Email.value.trim()
		};


		//call SignUp method in Account controller and transfer parameters with data: command
		$.ajax({
			dataType: 'json',
			type: 'POST',
			url: "/Account/SignUp",
			data: USUV1,
			success: function (obj1) {
				window.location.href = obj1.url + "?Message=" + obj1.message;
			},
			error: function (xhr, TextStatus, thrownError) {
				window.location.href = "/Home/UserList" + "?Message=" + xhr.status + ". " + thrownError;
			}
		});
	}
}


//---------------------------------------------------------
//check input fields in Add User dialig, if all fileds all filled, if 2 password fields are matching, is email regular
//-----------------------------------------------------
function CheckAddFields(InputFields){
	var AllFiledsRegular = true;

	//erase warning fields in modal
	$("#divAdd .alert-warning").empty();

	//loop through all properties of an object
	for (var key1 in InputFields) {
		//additional check is neccessery because object has other inherited properties aside from the ones assigned
		if (InputFields.hasOwnProperty(key1)) {
			var propVal= InputFields[key1].value.trim();//value of input field
			if (!propVal){//if field is empty
				var TableRow = InputFields[key1].parentNode.parentNode;//finds parent table row of the empty field (thats second parent, the first one is td cell with input field)
				var AlertField = TableRow.querySelector('.alert-warning');//get alert warning field from TableRow

				AlertField.innerHTML= "Required";
				AllFiledsRegular = false;
				break;
			}
		}
	}
	//if all fields have content, check if passwords are the same in passwprd and confirmpassword fields
	//check if email format is regular
	if (AllFiledsRegular) {
		var EmailVal= $("#add_Email").val().trim();
		if (!CheckMailFormat(EmailVal)) {
			$("#add_wrn_Email").text("Ivalid mail");
			AllFiledsRegular = false;
		}
	}
	return AllFiledsRegular;
}