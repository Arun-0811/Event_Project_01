// Get current path (excluding query string)
const currentPath = window.location.pathname.toLowerCase();

// Loop through all nav links
document.querySelectorAll(".navbar-nav .frst6 .nav-link").forEach(link => {
    const linkPath = link.getAttribute("href").toLowerCase();

    // Check if current path matches the link's path
    if (currentPath === linkPath || (currentPath === "/" && linkPath.includes("/home/index"))) {
        link.classList.add("active");
    } else {
        link.classList.remove("active");
    }
});


const Foothidden = document.getElementById("FootHide");

if (document.getElementById("navehome").classList.contains("active")) {
    Foothidden.style.visibility = "visible";
} else {
    Foothidden.style.visibility = "hidden";
}
