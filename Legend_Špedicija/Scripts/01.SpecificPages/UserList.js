//----------------------------------------------------------
// equivalent to document.ready function
//----------------------------------------------------------
$(document).ready(function () {
	//----------------------------------------------------------------
	//declaring function to initiate Edit dialog, define buttons, and their functions (read all boxes and call UpdateUser for "edit" button, close dialog for "close"
	//-----------------------------------------------------
	var EditDialog = function (type) {
		var title = type;
		$("#divEdit").dialog({//makes the dialog with divEdit html table
			autoOpen: false, modal: true, title: type + ' User',	width: 360,

			buttons: {//defines the function of the Save (read all fields and call UpdateUseran funct.) and Cancel buttons
				Save: function(){
					var JumpTo= Save_click();
				},
				Cancel: function () {$(this).dialog("destroy");}
			}
		});
	}

	//---------------------------------------------------------
	//-- called on Edit button click event. Called from HTML with: href= "javacript:void(0)" (means that doesnt refer to any function, it will be triggered on click event). Triggers because of lnkEdit class in the link element ("a") in html.
	//-----------------------------------------------------
	$("a.lnkEdit").on ("click", function () {
		EditDialog ("Edit");//call EditDialog function
		$(".alert-success").empty();//empties alert field in the users list
		var row = $(this).closest('tr');//finds first parent "tr" element of "this" ("a.lnkEdit" element, meaning button). So, it finds the row element in Users list table in which the "edit" button was clicked

		$("#hidID").val(row.find("td:eq(0)").html().trim());//set the value of "hidID" box with the value of the first (0) td element in the row. This field is hidden
		$("#txtLoginName").val(row.find("td:eq(1)").html().trim());
		$("#txtFirstName").val(row.find("td:eq(2)").html().trim());
		$("#txtLastName").val(row.find("td:eq(3)").html().trim());
		$("#txtEmail").val(row.find("td:eq(4)").text().trim());
		$("#ddlRoles").val(row.find("td:eq(6) > input").val().trim());
		$("#divEdit").dialog("open");//7th element (eq(6)) is td with hidden LookUpRoleID value. What does "> input" mean?
		return false;
	});



	//---------------------------------------------------------
	//-- triggered by pressing Enter on keyboard in Edit modal, jumps to Save_click()
	//-----------------------------------------------------
	$('#divEdit').keypress(function(event){
		if (event.which == 13){//enter code
			Save_click();
		}
	});

	//-----------------------------------------------------
	//--action on click Add User button
	//-----------------------------------------------------
	$("#AddUserButton").on ("click", function (){
		AddDialog();
		$(".alert-success").empty();
		return false;
	});

	//----------------------------------------------------------
	//declare AddUser dialog
	//-----------------------------------------------------
	var AddDialog= function (){
		$("#divAdd").dialog({
			autoOpen: true,
			modal: true,
			title: 'Add New User',
			width: 400,

			buttons: [{
				text: "Add",
				click: function(){
					var JumpTo= AddUser();
				}
			},{
				text: "Cancel",
				click: function(){$(this).dialog("destroy");}
			}]
		});
	}

	//---------------------------------------------------------
	//-- triggered by pressing Enter in AddUser modal, jumps to Save_click()
	//-----------------------------------------------------
	$('#divAdd').keypress(function(event){
		if (event.which == 13){//enter code
			AddUser();
		}
	});

	//---------------------------------------------------------
	//-- triggered on click of delete button
	//-----------------------------------------------------
	$("a.lnkDelete").on("click",
		function () {
			var row = $(this).closest('tr');
			var id = row.find("td:eq(0)").html().trim();//get user id from the first td cell
			var answer = confirm ("You are about to delete user with ID " + id + " . Continue?");//confirm dialog
			if (answer) DeleteUser(id);
			return false;
		}
	);
});//end of $(function ()