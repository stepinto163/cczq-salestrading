(function () {
  'use strict';

  let initialized = false;

  function getElements() {
    const page = document.getElementById('page-requirements');
    if (!page) return null;
    return {
      page,
      document: page.querySelector('.document'),
      nav: page.querySelector('.nav'),
      search: page.querySelector('.search'),
      top: page.querySelector('.top')
    };
  }

  function setupRequirementsReferencePage() {
    if (initialized) return;
    const elements = getElements();
    if (!elements || !elements.document || !elements.nav) return;

    const headings = Array.from(elements.document.querySelectorAll('h2, h3, h4'));
    const linksById = new Map();
    const searchable = Array.from(elements.document.querySelectorAll('h2, h3, h4, p, ol, ul, .table-wrap, .tbl-wrap, .flow, .flow-v'));

    headings.forEach((heading, index) => {
      if (!heading.id) heading.id = `requirements-heading-${index + 1}`;
      const link = document.createElement('a');
      link.href = `#${heading.id}`;
      link.dataset.target = heading.id;
      link.className = `level-${heading.tagName.slice(1)}`;
      link.textContent = heading.textContent.trim();
      elements.nav.appendChild(link);
      linksById.set(heading.id, link);
    });

    elements.nav.addEventListener('click', event => {
      const link = event.target.closest('a[data-target]');
      if (!link) return;
      event.preventDefault();
      const target = document.getElementById(link.dataset.target);
      if (target) target.scrollIntoView({ behavior: 'smooth', block: 'start' });
    });

    if (elements.search) {
      elements.search.addEventListener('input', () => {
        const query = elements.search.value.trim().toLocaleLowerCase();
        searchable.forEach(node => {
          const matched = !query || node.textContent.toLocaleLowerCase().includes(query);
          node.classList.toggle('hidden-by-search', !matched);
        });
      });
    }

    const updateActiveHeading = () => {
      const rootTop = elements.document.getBoundingClientRect().top + 18;
      let active = headings[0];
      headings.forEach(heading => {
        if (heading.getBoundingClientRect().top <= rootTop) active = heading;
      });
      linksById.forEach((link, id) => link.classList.toggle('active', id === active.id));
    };

    elements.document.addEventListener('scroll', () => {
      if (elements.top) elements.top.classList.toggle('visible', elements.document.scrollTop > 480);
      updateActiveHeading();
    }, { passive: true });

    if (elements.top) {
      elements.top.addEventListener('click', () => elements.document.scrollTo({ top: 0, behavior: 'smooth' }));
    }

    updateActiveHeading();
    initialized = true;
  }

  window.openRequirementsPage = setupRequirementsReferencePage;
})();
