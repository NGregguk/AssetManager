// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.

(() => {
  const storageKey = "asset-theme";
  const toggle = document.getElementById("theme-toggle");
  const root = document.body;

  if (!toggle || !root) {
    return;
  }

  const applyTheme = (theme) => {
    const isDark = theme === "dark";
    root.classList.toggle("dark-mode", isDark);
    toggle.setAttribute("aria-pressed", String(isDark));
    toggle.setAttribute("aria-label", isDark ? "Enable light mode" : "Enable dark mode");
  };

  const saved = localStorage.getItem(storageKey);
  if (saved === "dark" || saved === "light") {
    applyTheme(saved);
  } else if (window.matchMedia && window.matchMedia("(prefers-color-scheme: dark)").matches) {
    applyTheme("dark");
  } else {
    applyTheme("light");
  }

  toggle.addEventListener("click", () => {
    const nextTheme = root.classList.contains("dark-mode") ? "light" : "dark";
    localStorage.setItem(storageKey, nextTheme);
    applyTheme(nextTheme);
  });
})();
