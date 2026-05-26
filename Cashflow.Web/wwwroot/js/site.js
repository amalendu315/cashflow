(() => {
  const sidebar = document.getElementById("appSidebar");
  const toggleButton = document.querySelector("[data-sidebar-toggle]");

  if (!sidebar || !toggleButton) {
    return;
  }

  toggleButton.addEventListener("click", () => {
    sidebar.classList.toggle("show");
  });

  document.addEventListener("click", (event) => {
    const target = event.target;

    if (!(target instanceof HTMLElement)) {
      return;
    }

    const clickedInsideSidebar = sidebar.contains(target);
    const clickedToggle = toggleButton.contains(target);

    if (!clickedInsideSidebar && !clickedToggle) {
      sidebar.classList.remove("show");
    }
  });
})();
