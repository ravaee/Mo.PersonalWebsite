/**
 * Simple Image Picker for Quill.js Editor
 * Shows uploaded images in a modal and inserts selected image into editor
 */
class QuillImagePicker {
    constructor(quill) {
        this.quill = quill;
        this.images = [];
        this.init();
    }

    init() {
        this.setupImageHandler();
        this.createModal();
        this.loadImages();
    }

    /**
     * Sets up the image handler for Quill.js toolbar
     */
    setupImageHandler() {
        const toolbar = this.quill.getModule('toolbar');
        toolbar.addHandler('image', () => this.showImagePicker());
    }

    /**
     * Creates the image picker modal
     */
    createModal() {
        const modalHtml = `
            <div class="modal fade" id="imagePickerModal" tabindex="-1" aria-labelledby="imagePickerModalLabel" aria-hidden="true">
                <div class="modal-dialog modal-lg">
                    <div class="modal-content">
                        <div class="modal-header">
                            <h5 class="modal-title" id="imagePickerModalLabel">
                                <i class="bi bi-images me-2"></i>Select Image
                            </h5>
                            <button type="button" class="btn-close" data-bs-dismiss="modal" aria-label="Close"></button>
                        </div>
                        <div class="modal-body">
                            <div id="imagePickerLoading" class="text-center py-4">
                                <div class="spinner-border text-primary" role="status">
                                    <span class="visually-hidden">Loading...</span>
                                </div>
                                <p class="mt-2 text-muted">Loading images...</p>
                            </div>
                            <div id="imagePickerContent" class="d-none">
                                <div class="row mb-3">
                                    <div class="col">
                                        <a href="/Admin/Images" target="_blank" class="btn btn-outline-primary btn-sm">
                                            <i class="bi bi-upload me-1"></i>Upload New Images
                                        </a>
                                        <button type="button" class="btn btn-outline-secondary btn-sm ms-2" onclick="imagePickerInstance.loadImages()">
                                            <i class="bi bi-arrow-clockwise me-1"></i>Refresh
                                        </button>
                                    </div>
                                </div>
                                <div id="imagePickerGrid" class="row g-3">
                                    <!-- Images will be loaded here -->
                                </div>
                                <div id="imagePickerEmpty" class="text-center py-4 d-none">
                                    <i class="bi bi-images text-muted" style="font-size: 3rem;"></i>
                                    <h5 class="text-muted mt-3">No images found</h5>
                                    <p class="text-muted">Upload some images first using the button above.</p>
                                </div>
                            </div>
                        </div>
                    </div>
                </div>
            </div>
        `;

        document.body.insertAdjacentHTML('beforeend', modalHtml);
        this.modal = new bootstrap.Modal(document.getElementById('imagePickerModal'));
    }

    /**
     * Shows the image picker modal
     */
    showImagePicker() {
        this.modal.show();
        this.loadImages();
    }

    /**
     * Loads images from the API
     */
    async loadImages() {
        const loadingEl = document.getElementById('imagePickerLoading');
        const contentEl = document.getElementById('imagePickerContent');
        const gridEl = document.getElementById('imagePickerGrid');
        const emptyEl = document.getElementById('imagePickerEmpty');

        loadingEl.classList.remove('d-none');
        contentEl.classList.add('d-none');

        try {
            const response = await fetch('/api/Images');
            if (!response.ok) {
                throw new Error('Failed to load images');
            }

            this.images = await response.json();
            
            loadingEl.classList.add('d-none');
            contentEl.classList.remove('d-none');

            if (this.images.length === 0) {
                gridEl.innerHTML = '';
                emptyEl.classList.remove('d-none');
            } else {
                emptyEl.classList.add('d-none');
                this.renderImages();
            }
        } catch (error) {
            console.error('Error loading images:', error);
            loadingEl.innerHTML = `
                <div class="text-center py-4">
                    <i class="bi bi-exclamation-triangle text-warning" style="font-size: 3rem;"></i>
                    <h5 class="text-warning mt-3">Failed to load images</h5>
                    <p class="text-muted">Please try again or check your connection.</p>
                    <button type="button" class="btn btn-outline-primary btn-sm" onclick="imagePickerInstance.loadImages()">
                        <i class="bi bi-arrow-clockwise me-1"></i>Retry
                    </button>
                </div>
            `;
            loadingEl.classList.remove('d-none');
            contentEl.classList.add('d-none');
        }
    }

    /**
     * Renders the images in the grid
     */
    renderImages() {
        const gridEl = document.getElementById('imagePickerGrid');
        
        gridEl.innerHTML = this.images.map(image => `
            <div class="col-md-4 col-lg-3">
                <div class="card border-0 shadow-sm h-100 image-picker-card" style="cursor: pointer;" onclick="imagePickerInstance.selectImage('${image.filePath}', '${image.altText}')">
                    <div class="position-relative" style="height: 150px; overflow: hidden;">
                        <img src="${image.filePath}" 
                             alt="${image.altText}" 
                             class="card-img-top h-100" 
                             style="object-fit: cover;" 
                             loading="lazy" />
                    </div>
                    <div class="card-body p-2">
                        <h6 class="card-title text-truncate small mb-1" title="${image.originalFileName}">
                            ${image.originalFileName}
                        </h6>
                        ${image.width && image.height ? `<small class="text-muted">${image.width} × ${image.height} px</small>` : ''}
                    </div>
                </div>
            </div>
        `).join('');

        // Add hover effects
        const style = document.createElement('style');
        style.textContent = `
            .image-picker-card:hover {
                transform: translateY(-2px);
                transition: transform 0.2s ease;
                box-shadow: 0 4px 12px rgba(0,0,0,0.15) !important;
            }
        `;
        document.head.appendChild(style);
    }

    /**
     * Selects an image and inserts it into the editor
     */
    selectImage(imagePath, altText) {
        // Get current cursor position or end of document
        const range = this.quill.getSelection() || { index: this.quill.getLength() };
        
        // Insert the image
        this.quill.insertEmbed(range.index, 'image', imagePath);
        
        // Move cursor after the image
        this.quill.setSelection(range.index + 1);
        
        // Close the modal
        this.modal.hide();
        
        // Show success message
        this.showToast('Image inserted successfully!', 'success');
        
        // Focus back on editor
        this.quill.focus();
    }

    /**
     * Shows a toast notification
     */
    showToast(message, type = 'info') {
        // Create toast container if it doesn't exist
        let toastContainer = document.querySelector('.toast-container');
        if (!toastContainer) {
            toastContainer = document.createElement('div');
            toastContainer.className = 'toast-container position-fixed top-0 end-0 p-3';
            toastContainer.style.zIndex = '1055';
            document.body.appendChild(toastContainer);
        }

        // Create toast element
        const toastElement = document.createElement('div');
        toastElement.className = `toast align-items-center text-bg-${type} border-0`;
        toastElement.setAttribute('role', 'alert');
        toastElement.innerHTML = `
            <div class="d-flex">
                <div class="toast-body">${message}</div>
                <button type="button" class="btn-close btn-close-white me-2 m-auto" data-bs-dismiss="toast"></button>
            </div>
        `;

        toastContainer.appendChild(toastElement);

        // Initialize Bootstrap toast
        const toast = new bootstrap.Toast(toastElement, { autohide: true, delay: 3000 });
        toast.show();

        // Remove toast element after it's hidden
        toastElement.addEventListener('hidden.bs.toast', () => {
            toastElement.remove();
        });
    }
}

// Global instance for onclick handlers
let imagePickerInstance;

// Auto-initialize when Quill is ready
document.addEventListener('DOMContentLoaded', function() {
    // Wait for Quill to be initialized
    setTimeout(() => {
        if (window.quill) {
            imagePickerInstance = new QuillImagePicker(window.quill);
        }
    }, 500);
});