// Home.js - JavaScript functions for Home.razor
// Handles section switching, settings forms, and auth overlay

function showSection(section) {
    document.querySelectorAll('.section-view').forEach(el => el.classList.remove('active'));
    document.querySelectorAll('.settings-content').forEach(el => el.classList.add('hidden'));

    var sidebarId = 'sidebar-' + section;
    var feedId = 'feed-' + section;

    if (section === 'following') {
        sidebarId = 'sidebar-profile';
    }

    var sidebar = document.getElementById(sidebarId);
    var feed = document.getElementById(feedId);

    if (sidebar) sidebar.classList.add('active');
    if (feed) feed.classList.add('active');

    document.querySelectorAll('.main-nav a').forEach(el => el.classList.remove('active-link'));

    var navSection = section;
    if (section === 'following') navSection = 'profile';

    var navLink = document.getElementById('nav-' + navSection);
    if (navLink) navLink.classList.add('active-link');

    if (section === 'settings') {
        showSettingsForm('profile');
    }

    try {
        var headerSearchWrapper = document.getElementById('header-search-wrapper');
        var rightSidebar = document.getElementById('right-sidebar');
        var searchInput = document.getElementById('header-search-input');

        if (section === 'profile' || section === 'following') {
            if (headerSearchWrapper) headerSearchWrapper.style.display = 'flex';
            if (searchInput) searchInput.placeholder = 'Search users...';
            if (rightSidebar) rightSidebar.style.display = 'none';
        } else if (section === 'settings') {
            if (headerSearchWrapper) headerSearchWrapper.style.display = 'none';
            if (rightSidebar) rightSidebar.style.display = 'none';
            showSettingsForm('profile');
        } else {
            if (headerSearchWrapper) headerSearchWrapper.style.display = 'flex';
            if (searchInput) searchInput.placeholder = 'Search posts...';
            if (rightSidebar) rightSidebar.style.display = 'flex';
        }
    } catch (e) { }

    window.currentSection = section;
}

function showSettingsForm(formName) {
    document.querySelectorAll('.settings-content').forEach(el => {
        el.classList.add('hidden');
    });
    var target = document.getElementById('settings-form-' + formName);
    if (target) {
        target.classList.remove('hidden');
    }
    document.querySelectorAll('.sidebar-section .category-link').forEach(el => el.classList.remove('active-setting'));
    var link = document.getElementById('set-link-' + formName);
    if (link) link.classList.add('active-setting');
}

function showLoginOverlay() {
    document.getElementById('auth-overlay').style.display = 'flex';
}

function toggleAuthMode() {
    var login = document.getElementById('login-view');
    var reg = document.getElementById('register-view');
    if (login.classList.contains('hidden')) {
        login.classList.remove('hidden');
        reg.classList.add('hidden');
    } else {
        login.classList.add('hidden');
        reg.classList.remove('hidden');
    }
}
