document.addEventListener("DOMContentLoaded", function () {
    const menuItems = document.querySelectorAll(".sidebar-menu li a");

    // Set default page to "Dashboard" if no active page is stored
    if (!localStorage.getItem("activePage")) {
        localStorage.setItem("activePage", window.location.origin + "/Dashboard");
    }

    menuItems.forEach(item => {
        item.addEventListener("click", function () {
            // Store the clicked page in localStorage
            localStorage.setItem("activePage", this.href);
        });
    });

    // Ensure the correct menu item is highlighted after refresh
    const currentPage = localStorage.getItem("activePage");
    if (currentPage) {
        menuItems.forEach(item => {
            if (item.href === currentPage) {
                item.parentElement.classList.add("active");
            }
        });
    }
});
