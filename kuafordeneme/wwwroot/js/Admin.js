document.addEventListener("DOMContentLoaded", function () {
    // İşlem Yönetimi Tablosunu Yükle
    loadIslemTable();

    // Sekme değiştiğinde veriyi dinamik yükle
    document.getElementById("adminTabs").addEventListener("click", function (e) {
        if (e.target.id === "islem-tab") {
            loadIslemTable();
        } else if (e.target.id === "calisan-tab") {
            loadCalisanTable();
        } else if (e.target.id === "randevu-tab") {
            loadRandevuTable();
        } else if (e.target.id === "kullanici-tab") {
            loadKullaniciTable();
        }
    });
});

function loadIslemTable() {
    fetch("/Admin/GetIslemTable")
        .then(response => response.text())
        .then(html => {
            document.getElementById("islemTable").innerHTML = html;
        });
}

function loadCalisanTable() {
    fetch("/Admin/GetCalisanTable")
        .then(response => response.text())
        .then(html => {
            document.getElementById("calisanTable").innerHTML = html;
        });
}

function loadRandevuTable() {
    fetch("/Admin/GetRandevuTable")
        .then(response => response.text())
        .then(html => {
            document.getElementById("randevuTable").innerHTML = html;
        });
}

function loadKullaniciTable() {
    fetch("/Admin/GetKullaniciTable")
        .then(response => response.text())
        .then(html => {
            document.getElementById("kullaniciTable").innerHTML = html;
        });
}
