// Sayfa içeriği tamamen yüklendiğinde çalıştır
document.addEventListener('DOMContentLoaded', () => {
    
    // 1. DEĞİŞKENLERİ TANIMLA
    const navLinks = document.querySelectorAll('.nav-link');
    const sections = document.querySelectorAll('section, header');
    const navbarHeight = 80; // CSS'teki navbar yüksekliğinle aynı olmalı

    // 2. YUMUŞAK KAYDIRMA (SMOOTH SCROLL)
    navLinks.forEach(link => {
        link.addEventListener('click', (e) => {
            const href = link.getAttribute('href');

            // Sadece sayfa içi linkleri (# ile başlayanlar) işle
            if (href.startsWith('#')) {
                e.preventDefault();
                const targetId = href.substring(1);
                const targetElement = document.getElementById(targetId);

                if (targetElement) {
                    const elementPosition = targetElement.offsetTop;
                    // Navbar yüksekliğini hesaba katarak kaydır
                    window.scrollTo({
                        top: elementPosition - (navbarHeight - 10),
                        behavior: 'smooth'
                    });
                }
            }
        });
    });

    // 3. SCROLL SPY (KAYDIRDIKÇA MENÜYÜ GÜNCELLE)
    const changeActiveLink = () => {
        let currentSectionId = "";

        // Sayfanın en altında mıyız kontrolü (İletişim linki için kritik)
        if (window.innerHeight + window.scrollY >= document.body.offsetHeight - 50) {
            currentSectionId = "contact";
        } else {
            sections.forEach(section => {
                const sectionTop = section.offsetTop;
                const sectionHeight = section.clientHeight;
                
                // Eğer ekranın üstü bölümün içindeyse ID'yi al
                if (window.scrollY >= sectionTop - navbarHeight - 20) {
                    currentSectionId = section.getAttribute('id');
                }
            });
        }

        // Menüdeki linkleri güncelle
        navLinks.forEach(link => {
            link.classList.remove('active');
            // href içindeki ID ile o anki bölüm eşleşiyorsa active ekle
            if (link.getAttribute('href') === `#${currentSectionId}`) {
                link.classList.add('active');
            }
        });
    };

    // Sayfa her kaydırıldığında fonksiyonu çalıştır
    window.addEventListener('scroll', changeActiveLink);
    
    // Sayfa yenilendiğinde hangi bölümde olduğunu kontrol et
    changeActiveLink();
});