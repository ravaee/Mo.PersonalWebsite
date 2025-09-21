// Interactive Image Hover Messages
class ImageHoverManager {
    constructor() {
        this.messages = [
            "Hi there! 👋",
            "What's up? 🤔",
            "Check out my articles! 📚",
            "Connect with me on LinkedIn! 💼",
            "Let's code something awesome! 💻",
            "Gaming later? 🎮",
            "Time for the gym! 💪",
            "Coffee break? ☕",
            "Debugging life... 🐛",
            "404: Joke not found 😄",
            "Hello, World! 🌍",
            "Ctrl+Z my mistakes please 🔄",
            "Loading... please wait ⏳",
            "You found the easter egg! 🥚",
            "Read my About page! 📄",
            "Follow me for more code! 👨‍💻"
        ];
        
        this.lastMessageIndex = -1;
        this.init();
    }

    init() {
        const images = document.querySelectorAll('.interactive-image');
        images.forEach(img => this.setupImageHover(img));
    }

    setupImageHover(img) {
        // Wrap image in container if not already wrapped
        if (!img.parentElement.classList.contains('image-hover-container')) {
            const container = document.createElement('div');
            container.className = 'image-hover-container';
            img.parentNode.insertBefore(container, img);
            container.appendChild(img);
            
            // Create message element
            const messageElement = document.createElement('div');
            messageElement.className = 'hover-message';
            container.appendChild(messageElement);
            
            // Add event listeners
            container.addEventListener('mouseenter', () => this.showMessage(messageElement));
            container.addEventListener('mouseleave', () => this.hideMessage(messageElement));
            container.addEventListener('click', () => this.createSparkles(container));
        }
    }

    showMessage(messageElement) {
        const randomMessage = this.getRandomMessage();
        messageElement.textContent = randomMessage;
    }

    hideMessage(messageElement) {
        // Message hides automatically via CSS
    }

    getRandomMessage() {
        let randomIndex;
        do {
            randomIndex = Math.floor(Math.random() * this.messages.length);
        } while (randomIndex === this.lastMessageIndex && this.messages.length > 1);
        
        this.lastMessageIndex = randomIndex;
        return this.messages[randomIndex];
    }

    createSparkles(container) {
        const rect = container.getBoundingClientRect();
        const sparkleCount = 5;
        
        for (let i = 0; i < sparkleCount; i++) {
            setTimeout(() => {
                const sparkle = document.createElement('div');
                sparkle.className = 'sparkle';
                
                // Random position around the image
                const x = Math.random() * rect.width;
                const y = Math.random() * rect.height;
                
                sparkle.style.left = x + 'px';
                sparkle.style.top = y + 'px';
                
                container.appendChild(sparkle);
                
                // Trigger animation
                setTimeout(() => sparkle.classList.add('animate'), 10);
                
                // Remove sparkle after animation
                setTimeout(() => {
                    if (sparkle.parentNode) {
                        sparkle.parentNode.removeChild(sparkle);
                    }
                }, 1500);
                
            }, i * 100);
        }
    }
}

// Initialize when DOM is loaded
document.addEventListener('DOMContentLoaded', () => {
    new ImageHoverManager();
});

// Export for potential future use
window.ImageHoverManager = ImageHoverManager;