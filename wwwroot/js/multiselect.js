document.addEventListener("DOMContentLoaded", () => {
    let DDLforChosenTag = document.getElementById("selectedOptionsTag");
    let DDLforAvailTag = document.getElementById("availOptionsTag");

    let DDLforChosenSector = document.getElementById("selectedOptionsSector");
    let DDLforAvailSector = document.getElementById("availOptionsSector");

    let DDLforChosenMembership = document.getElementById("selectedOptionsMembership");
    let DDLforAvailMembership = document.getElementById("availOptionsMembership");

    let DDLforChosenNaicsCode = document.getElementById("selectedOptionsNaicsCode");
    let DDLforAvailNaicsCode = document.getElementById("availOptionsNaicsCode");

    // Function to switch list items from one dropdown to another
    function switchOptions(event, senderDDL, receiverDDL) {
        let senderID = senderDDL.id;
        let selectedOptions = document.querySelectorAll(`#${senderID} option:checked`);
        //console.log("Sender DDl:", senderDDL); // Debugging
        //console.log("receiver DDL:", receiverDDL); // Debugging

        event.preventDefault();

        if (selectedOptions.length === 0) {
            alert("Nothing to move.");
        } else {
            selectedOptions.forEach((o) => {
                senderDDL.remove(o.index);
                receiverDDL.appendChild(o);
            });
        }
    }

    // Create closures for Tags
    let addOptionsTag = (event) => switchOptions(event, DDLforAvailTag, DDLforChosenTag);
    let removeOptionsTag = (event) => switchOptions(event, DDLforChosenTag, DDLforAvailTag);

    // Create closures for Sectors
    let addOptionsSector = (event) => switchOptions(event, DDLforAvailSector, DDLforChosenSector);
    let removeOptionsSector = (event) => switchOptions(event, DDLforChosenSector, DDLforAvailSector);

    // Create closures for Membership
    let addOptionsMembership = (event) => switchOptions(event, DDLforAvailMembership, DDLforChosenMembership);
    let removeOptionsMembership = (event) => switchOptions(event, DDLforChosenMembership, DDLforAvailMembership);

    // Create closures for NAICS Codes
    let addOptionsNaicsCode = (event) => switchOptions(event, DDLforAvailNaicsCode, DDLforChosenNaicsCode);
    let removeOptionsNaicsCode = (event) => switchOptions(event, DDLforChosenNaicsCode, DDLforAvailNaicsCode);

    // Assign event handlers for each button
    document.getElementById("btnLeftTag").addEventListener("click", addOptionsTag);
    document.getElementById("btnRightTag").addEventListener("click", removeOptionsTag);

    document.getElementById("btnLeftSector").addEventListener("click", addOptionsSector);
    document.getElementById("btnRightSector").addEventListener("click", removeOptionsSector);

    document.getElementById("btnLeftMembership").addEventListener("click", addOptionsMembership);
    document.getElementById("btnRightMembership").addEventListener("click", removeOptionsMembership);

    document.getElementById("btnLeftNaicsCode").addEventListener("click", addOptionsNaicsCode);
    document.getElementById("btnRightNaicsCode").addEventListener("click", removeOptionsNaicsCode);

    document.getElementById("btnSubmit").addEventListener("click", function () {
        // Ensure all selected options in the chosen lists are marked as selected before submitting the form
        DDLforChosenTag.childNodes.forEach(opt => opt.selected = "selected");
        DDLforChosenSector.childNodes.forEach(opt => opt.selected = "selected");
        DDLforChosenMembership.childNodes.forEach(opt => opt.selected = "selected");
        DDLforChosenNaicsCode.childNodes.forEach(opt => opt.selected = "selected");
    });
});
