// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.
document.addEventListener('DOMContentLoaded', () => {
    const updateIcon = (theme) => {
        const icon = document.getElementById('theme-icon');
        if (icon) {
            if (theme === 'dark') {
                icon.className = 'bi bi-sun-fill';
            } else {
                icon.className = 'bi bi-moon-stars-fill';
            }
        }
    };

    updateIcon(document.documentElement.getAttribute('data-bs-theme'));

    document.getElementById('theme-toggle')?.addEventListener('click', () => {
        const currentTheme = document.documentElement.getAttribute('data-bs-theme');
        const newTheme = currentTheme === 'dark' ? 'light' : 'dark';
        document.documentElement.setAttribute('data-bs-theme', newTheme);
        localStorage.setItem('theme', newTheme);
        updateIcon(newTheme);
    });

    // Auto-inject Show/Hide Password toggles into all Bootstrap floating password inputs
    document.querySelectorAll('.form-floating input[type="password"]').forEach(input => {
        // Prevent double injection if run multiple times
        if (input.parentElement.querySelector('.password-toggle')) return;

        const toggleBtn = document.createElement('i');
        toggleBtn.className = 'bi bi-eye password-toggle';
        toggleBtn.style.position = 'absolute';
        toggleBtn.style.right = '15px';
        toggleBtn.style.top = '50%';
        toggleBtn.style.transform = 'translateY(-50%)';
        toggleBtn.style.cursor = 'pointer';
        toggleBtn.style.zIndex = '10'; // Above the floating label
        toggleBtn.style.fontSize = '1.2rem';
        toggleBtn.style.color = 'var(--bs-secondary-color)';

        // Add padding so long passwords don't hide behind the icon
        input.style.paddingRight = '2.5rem';

        toggleBtn.addEventListener('click', () => {
            if (input.type === 'password') {
                input.type = 'text';
                toggleBtn.className = 'bi bi-eye-slash password-toggle';
            } else {
                input.type = 'password';
                toggleBtn.className = 'bi bi-eye password-toggle';
            }
        });

        input.parentElement.style.position = 'relative';
        input.parentElement.appendChild(toggleBtn);
    });
});
