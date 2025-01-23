# Hotel Paradise

Aplikacja webowa umożliwiająca zarządzanie hotelem – rezerwacjami, pokojami, usługami, posiłkami i kontami użytkowników (z różnymi rolami i uprawnieniami).

## Autorzy / Zespół

- **Dziyana Volkava**  
- **Natalia Rafałowska**

## Spis treści

1. [Opis projektu](#opis-projektu)
2. [Wykorzystane technologie / biblioteki](#wykorzystane-technologie--biblioteki)
3. [Funkcjonalności (CRUD-y i inne)](#funkcjonalności-crud-y-i-inne)
4. [Instrukcja instalacji i uruchomienia](#instrukcja-instalacji-i-uruchomienia)
5. [Struktura projektu](#struktura-projektu)
6. [Uwierzytelnianie i role](#uwierzytelnianie-i-role)
7. [Przesyłanie plików](#przesyłanie-plików)
8. [Migracje i ORM](#migracje-i-orm)
9. [Dodatkowe uwagi](#dodatkowe-uwagi)
10. [Licencja](#licencja)


---

## Opis projektu

**Hotel Paradise** to aplikacja napisana w technologii .NET 8 / ASP.NET Core 8, która umożliwia:

- Rezerwację pokoi (zarówno rezerwacje pojedyncze, jak i grupowe).
- Zarządzanie listą dostępnych pokoi, ich blokadą, ceną itp.
- Zarządzanie podstawowymi danymi hotelu (np. typ zakwaterowania, pakiety zakwaterowań, kategorie dań, dania).
- Uwierzytelnianie oraz autoryzację użytkowników w oparciu o różne role (np. Admin, Recepcja, użytkownik zalogowany).
- Przesyłanie zdjęć (plików) do galerii pokoi, dań itp.
- Podstawowe operacje CRUD na większości encji (formularze + widoki list + szczegóły).
- Wyświetlanie oferty hotelu i dań z poziomu strony publicznej (bez logowania).
- Administracja (dashboard) dostępna tylko dla ról o wyższych uprawnieniach.

Projekt jest realizowany jako aplikacja wielowarstwowa, z wykorzystaniem wzorca **MVC** (Model-View-Controller). W warstwie danych zastosowano Entity Framework Core (ORM). Migracje pozwalają na szybkie tworzenie/aktualizację bazy danych.


## Wykorzystane technologie / biblioteki

| Technologia / Biblioteka          | Wersja        | Opis                                                                                          |
|-----------------------------------|---------------|-----------------------------------------------------------------------------------------------|
| **.NET / ASP.NET Core**           | 8 0           | Platforma do tworzenia aplikacji webowych                                                     |
| **Entity Framework Core**         | 8.0           | ORM do komunikacji z bazą danych                                                              |
| **Microsoft Identity**            |8.0            | Uwierzytelnianie, zarządzanie użytkownikami i rolami                                          |
| **SQL Server**                    | 2019+         | Silnik bazodanowy, w tym projekt korzysta z bazy m.in. do przechowywania danych hotelowych    |
| **Hangfire**                      | 1.8.17        | Harmonogram zadań w tle (m.in. automatyczne zwalnianie 
  pokoi itp.)                       |
| **Bootstrap**                     | 5.x           | Biblioteka CSS/JS – stylowanie widoków                                                        |
| **jQuery**                        | 3.6.x         | Manipulacje DOM, AJAX                                                                         |
| **Owl Carousel / Flexslider**     | (dostarczone) | Dodatkowe efekty graficzne i slidery na stronie                                               |
| **Font Awesome / Icomoon**        | (dostarczone) | Ikony SVG                                                                                     



## Funkcjonalności (CRUD-y i inne)

W projekcie zaimplementowano:

1. **Obsługa zakwaterowań (CRUD)**  
   - Tworzenie, edycja, usuwanie i wyświetlanie listy.
   - Relacja OneToMany (zakwaterowania – pokoje).
   - Relacja ManyToMany (rezerwacje grupowe – pokoje).

2. **Obsługa pakietów zakwaterowań (CRUD)**  
   - Dodawanie, edycja, usuwanie, przeglądanie.
   - Relacja OneToMany (typ zakwaterowania – pakiety).

3. **Obsługa kategorii dań (CRUD)**  
   - Dodawanie, edycja, usuwanie, przeglądanie.

4. **Obsługa dań (CRUD)**  
   - Dodawanie, edycja, usuwanie, przeglądanie.
   - Przypisywanie zdjęć.

5. **Rezerwacje (CRUD)**  
   - Rezerwacje pojedyncze (Room -> Reservation).
   - Rezerwacje grupowe (przykład ManyToMany: GroupReservation <-> Room).
   - Automatyczne zwalnianie pokoi przez Hangfire.

6. **Uwierzytelnianie i autoryzacja**  
   - Logowanie / rejestracja.
   - Role: Admin, Recepcja, Użytkownik.

7. **Przesyłanie plików**  
   - Możliwość wgrywania zdjęć do pokoi i dań (obsługa tagu `<input type="file">`).

8. **Publiczne strony**  
   - Strona główna, widok dań i rezerwacji z poziomu gościa (bez logowania).

9. **Wyszukiwarka pokoi (podstawowa)**  
   - Zakres dat, liczba osób, wyświetlanie dostępnych pokoi.


## Instrukcja instalacji i uruchomienia

1. **Klonowanie repozytorium**  
   ```bash
   git clone https://github.com/dianaliv95/HotelP.git
   cd HotelP

2. **Przygotowanie bazy danych**

   W pliku appsettings.json skonfiguruj połączenie do serwera SQL w sekcji "ConnectionStrings" -> 
   "HMSConnection".
    Opcjonalnie dostosuj parametry bazy (nazwa, user, hasło).

3. **Migracje**

    Uruchom w konsoli menedżera pakietów (lub w terminalu) polecenie:
    **dotnet ef database update**
     Spowoduje to utworzenie bazy danych i struktury tabel.

4. **Uruchomienie aplikacji**

    W katalogu głównym projektu wydaj polecenie:
   **dotnet run**
  Otwórz w przeglądarce adres, który wyświetli się w konsoli (np. https://localhost:5001).

5. **Logowanie**

    Po uruchomieniu aplikacji można się zalogować za pomocą wstępnie utworzonego konta administratora (o ile        jest seed).
    Domyślny login administratora: admin@hotel.com, hasło: adminadmin1.
    (Może się różnić w zależności od plików seedujących – sprawdź w kodzie.)

6. **Struktura projektu**
   
   - **HMS.Data**
      Zawiera kontekst bazy danych HMSContext, migracje i konfiguracje EF Core.
     
   - **HMS.Entities**
      Modele encji (klasy C# odwzorowujące tabele w bazie).
     
   - **HMS.Services**
      Logika biznesowa i dostęp do danych. Przykłady: AccommodationService, DishesService, BookingService itp.
 
   - **Hotel / HotelP (warstwa prezentacji)**
        **Controllers** – kontrolery MVC, podzielone m.in. na obszary (Areas).
        **Views** – widoki Razor, zorganizowane w foldery odpowiadające kontrolerom.
        **wwwroot** – zasoby statyczne (CSS, JS, obrazki).
        **Areas** – folder z obszarami (Dashboard, RestaurantManagement itp.).
     
7. **Uwierzytelnianie i role**
    Zastosowano standardowy mechanizm **Identity** (ClaimsIdentity).
   
    Projekt wspiera co najmniej dwie role:
    **Admin** – pełny dostęp do panelu zarządzania.
    **Recepcja** – dostęp do części funkcji administracyjnych (rezerwacje).

W Program.cs skonfigurowano services.AddIdentity<,>(), a w widokach i kontrolerach użyto [Authorize(Roles = "...")] do ograniczania dostępu.

8 **Przesyłanie plików**
  W systemie stworzono formatki (<input type="file">) pozwalające wgrywać zdjęcia.
  W DashboardController (lub innych kontrolerach) zdefiniowano akcję UploadPictures, która odbiera pliki i     
  zapisuje w folderze wwwroot/images/site/.
  Identyfikatory i ścieżki do plików przechowywane są w tabeli Pictures w bazie.
  
9. **Migracje i ORM**
    Użyto **Entity Framework Core** do mapowania obiektowo-relacyjnego.
   
    W pliku HMSContext.cs zdefiniowano DbSety (np. public DbSet<Accommodation> Accommodations { get; set; }).
    W OnModelCreating konfigurowane są relacje (OneToMany, ManyToMany, restrykcje usuwania itd.).
    Migracje – pliki w folderze Migrations. Komenda dotnet ef migrations add <nazwa_migracji> generuje nową 
    migrację.


10. **Dodatkowe uwagi**
Kod jest udostępniony w repozytorium: GitHub: dianaliv95/HotelP.
Projekt zawiera także Hangfire do cyklicznych zadań (np. AutoReleaseJob zwalniający pokoje po zakończeniu rezerwacji).

11. **Licencja**
Ten projekt jest chroniony prawami autorskimi. Wszelkie prawa zastrzeżone (2025).
Zastosowanie wyłącznie w celach dydaktycznych.


   



