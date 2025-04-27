// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
const btnBackToTop = document.getElementById('btnBackToTop')
const footer = document.getElementById('footer-scroll')

function BackToTop() {
    window.scrollTo({
        top: 0,
        behavior: 'smooth' // Adds a smooth scroll effect
    })
}
footer.addEventListener('click', () => {
    BackToTop()
})
btnBackToTop.addEventListener('click', () => {
    BackToTop()
})


function toggleVisibility(id) {
    var element = document.getElementById(id);

    if (element.style.opacity === "0" || element.style.display === "none" || element.style.opacity === "") {
        element.style.display = "block";
        setTimeout(() => {
            element.style.opacity = "1";
        }, 10); // Small delay to allow transition
    } else {
        element.style.opacity = "0";
        setTimeout(() => {
            element.style.display = "none";
        }, 300); // Matches transition duration
    }
}



