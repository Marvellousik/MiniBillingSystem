USE InternBillingDB;
GO

-- 1. Clear existing data
DELETE FROM Payments;
DELETE FROM Bills;
DELETE FROM Customers;
GO

-- 2. Insert 90 Clean, Authentic AMAC (Abuja Municipal Area Council) Customers
SET IDENTITY_INSERT Customers ON;

INSERT INTO Customers (CustomerID, FullName, Address, PhoneNumber, Email, AccountBalance) VALUES
(1, N'Emeka Okafor', N'Plot 142, Aminu Kano Crescent, Wuse II, Abuja', N'08033124590', N'emeka.okafor@gmail.com', 5000.00),
(2, N'Hauwa Mohammed Bello', N'No. 8, Crescent 4, Kado Estate, Abuja', N'08124567891', N'hmbello@yahoo.com', 0.00),
(3, N'Adebayo Chukwuma Adeleke', N'Block 12, Flat 3, FHA Estate, Gwarinpa, Abuja', N'07061234567', N'bayo.adeleke@outlook.com', 12500.50),
(4, N'Fatima Garba Aliyu', N'Shop 15, Area 1 Shopping Complex, Garki, Abuja', N'08059876543', N'fatima.aliyu@garki-biz.ng', 0.00),
(5, N'Chidiebere Nnamdi Okonkwo', N'No. 24, Obafemi Awolowo Way, Jabi, Abuja', N'08187654321', N'c.okonkwo@gmail.com', 2500.00),
(6, N'Aisha Ibrahim Danjuma', N'Suite B12, Banex Plaza, Wuse 2, Abuja', N'08023456789', N'aisha.danjuma@hotmail.com', 0.00),
(7, N'Babatunde Olumide Johnson', N'House 7, 3rd Avenue, Model City, Lugbe, Abuja', N'08139988776', N'babs.johnson@gmail.com', 0.00),
(8, N'Nkechi Blessing Ezekwesili', N'Plot 402, Cadastral Zone B06, Mabushi, Abuja', N'07031122334', N'nkechi.blessing@yahoo.com', 7500.00),
(9, N'Umar Farouq Yakubu', N'No. 5, Shehu Shagari Way, Maitama, Abuja', N'08094455667', N'uf.yakubu@amaccouncil.gov.ng', 0.00),
(10, N'Grace Ifeoma Nwachukwu', N'Corner Shop 4, Karu Market Road, Karu, Abuja', N'08162233445', N'grace.nwachukwu@gmail.com', 1500.00),

(11, N'Yakubu Haruna Mustapha', N'Plot 88, Yakubu Gowon Crescent, Asokoro, Abuja', N'08035544332', N'y.mustapha@asokorolaw.ng', 25000.00),
(12, N'Blessing Chinyere Egwu', N'No. 19, Gimbiya Street, Area 11, Garki, Abuja', N'08147788990', N'blessing.egwu@gmail.com', 0.00),
(13, N'Kabiru Usman Bichi', N'Block B, Suite 4, Utako Modern Market, Utako, Abuja', N'07089900112', N'kabiru.bichi@yahoo.com', 0.00),
(14, N'Oluwaseun Timothy Fashola', N'House 44, 4th Avenue, Gwarinpa, Abuja', N'08051122334', N'seun.fashola@gmail.com', 8400.00),
(15, N'Zainab Idris Abubakar', N'Plot 501, Aguiyi Ironsi Street, Maitama, Abuja', N'08173344556', N'zainab.idris@abubakar.com', 0.00),
(16, N'Kingsley Osas Ighodaro', N'No. 3, Alex Ekwueme Way, Jabi, Abuja', N'08027766554', N'k.ighodaro@gmail.com', 3200.00),
(17, N'Hadiza Salisu Shehu', N'House 12, Road 14, Federal Housing, Lugbe, Abuja', N'08108899001', N'hadiza.salisu@yahoo.com', 0.00),
(18, N'Tunde Rasheed Bakare', N'Plot 72, Kwame Nkrumah Crescent, Asokoro, Abuja', N'07054433221', N'tunde.bakare@bakare-corp.com', 45000.00),
(19, N'Chioma Stephanie Nnaji', N'Suite 102, Emab Plaza, Wuse 2, Abuja', N'08032211009', N'chioma.nnaji@gmail.com', 0.00),
(20, N'Suleiman Ahmed Galadima', N'No. 88, Lokoja Street, Area 8, Garki, Abuja', N'08164455667', N's.galadima@gmail.com', 0.00),

(21, N'Funmilayo Toyin Adewale', N'House 15, Close 3, Sun City Estate, Galadimawa, Abuja', N'08091122334', N'funmi.adewale@gmail.com', 1800.00),
(22, N'Bashir Mahmoud Gwandu', N'Plot 204, Cadastral Zone A09, Guzape, Abuja', N'08129988776', N'bashir.gwandu@gwandu.ng', 0.00),
(23, N'Kelechi Donald Uzoma', N'Block E, Shop 9, Wuse Market, Wuse 1, Abuja', N'07038877665', N'kelechi.uzoma@gmail.com', 6200.00),
(24, N'Aminu Sani Bello', N'House 9, Road 22, Life Camp, Abuja', N'08063344556', N'aminu.bello@yahoo.com', 0.00),
(25, N'Joy Omoefe Efe', N'Flat 4, Block 8, CBN Estate, Karu, Abuja', N'08182233445', N'joy.efe@gmail.com', 0.00),
(26, N'Musa Dahiru Katagum', N'Plot 110, Ahmadu Bello Way, Central Business District, Abuja', N'08029900112', N'm.katagum@cbd-tech.ng', 150000.00),
(27, N'Ifeanyi Christopher Anyanwu', N'House 31, 1st Avenue, Kubwa, Abuja', N'08114455667', N'ifeanyi.anyanwu@gmail.com', 0.00),
(28, N'Ruqayya Umar Ganduje', N'No. 14, Nelson Mandela Street, Asokoro, Abuja', N'07085566778', N'ruqayya.ganduje@yahoo.com', 0.00),
(29, N'Damilola Victor Ogunleye', N'Suite 304, Sahad Stores Building, Central Area, Abuja', N'08037788990', N'dami.ogunleye@gmail.com', 4100.00),
(30, N'Halima Kabir Katsina', N'House 5, Road 7, Games Village, Kaura, Abuja', N'08152233445', N'halima.katsina@gmail.com', 0.00),

(31, N'Nonso Kingsley Ezeugo', N'Plot 14, Solomon Lar Way, Utako, Abuja', N'08031122334', N'nonso.ezeugo@gmail.com', 0.00),
(32, N'Zubaida Lawal Daura', N'House 18, 5th Avenue, City College Road, Karu, Abuja', N'08064433221', N'zubaida.daura@yahoo.com', 0.00),
(33, N'Abubakar Sadeeq Turaki', N'No. 77, Tafawa Balewa Way, Area 3, Garki, Abuja', N'08132211009', N'sadeeq.turaki@turakiholdings.ng', 55000.00),
(34, N'Patricia Ngozi Nwosu', N'House 3, Close 12, Prince and Princess Estate, Duboyi, Abuja', N'07049988776', N'patricia.nwosu@gmail.com', 0.00),
(35, N'Ibrahim Mansur Yola', N'Plot 805, Constitution Avenue, Central Area, Abuja', N'08021122334', N'ibrahim.yola@gmail.com', 1200.00),
(36, N'Folake Abigail Ojo', N'No. 42, Haile Selassie Street, Asokoro, Abuja', N'08178899001', N'folake.ojo@gmail.com', 0.00),
(37, N'Chinedu Emmanuel Offor', N'Block C, Flat 2, Apo Legislative Quarters, Apo, Abuja', N'08039900112', N'chinedu.offor@gmail.com', 31000.00),
(38, N'Maryam Aliyu Gusau', N'House 22, Road 3, Citec Estate, Mbora, Abuja', N'08145566778', N'maryam.gusau@yahoo.com', 0.00),
(39, N'Oche Peter Agada', N'Plot 55, Adetokunbo Ademola Crescent, Wuse 2, Abuja', N'07062233445', N'oche.agada@gmail.com', 950.00),
(40, N'Khadijah Nuhu Ribadu', N'No. 11, Hon. Justice Udo Udoma Street, Asokoro, Abuja', N'08058899001', N'khadijah.ribadu@gmail.com', 0.00),

(41, N'Tochukwu Gerald Nzeogwu', N'House 8, 2nd Avenue, Model Estate, Lugbe, Abuja', N'08184455667', N'tochukwu.nzeogwu@gmail.com', 0.00),
(42, N'Safiya Mohammed Manga', N'No. 6, Baturaye Street, Area 2, Garki, Abuja', N'08025544332', N'safiya.manga@gmail.com', 0.00),
(43, N'Rotimi Francis Adegoke', N'Plot 312, Cadastral Zone B04, Mabushi, Abuja', N'08101122334', N'rotimi.adegoke@adegoke.ng', 14500.00),
(44, N'Amina Rabiu Kwankwaso', N'House 17, Close 5, NAF Valley Estate, Asokoro, Abuja', N'07039988776', N'amina.kwankwaso@gmail.com', 0.00),
(45, N'Chukwuemeka David Nwoye', N'Suite 12, Area 7 Shopping Complex, Garki, Abuja', N'08068877665', N'emeka.nwoye@gmail.com', 0.00),
(46, N'Zainab Hassan Zaria', N'House 9, 6th Avenue, Gwarinpa, Abuja', N'08134455667', N'zainab.zaria@yahoo.com', 5200.00),
(47, N'Olumide Gabriel Sowore', N'Plot 99, Umaru Dikko Street, Jabi, Abuja', N'08092233445', N'sowore.olumide@gmail.com', 0.00),
(48, N'Asmau Garba Shehu', N'No. 3, Douala Street, Wuse Zone 5, Abuja', N'08127788990', N'asmau.shehu@gmail.com', 0.00),
(49, N'Uchechukwu Fidelis Igwe', N'House 14, Road 10, Federal Housing Estate, Karu, Abuja', N'07051122334', N'uche.igwe@gmail.com', 2900.00),
(50, N'Rabiatou Saidou Bello', N'Plot 602, Cadastral Zone B12, Katampe Extension, Abuja', N'08034455667', N'rabiatou.bello@gmail.com', 0.00),

(51, N'Nura Muhammad Danbatta', N'House 21, Road 4, Trademore Estate, Lugbe, Abuja', N'08149900112', N'nura.danbatta@gmail.com', 0.00),
(52, N'Adaeze Maureen Ekwueme', N'Suite B08, Preferred Plaza, Utako, Abuja', N'08028877665', N'adaeze.ekwueme@gmail.com', 6700.00),
(53, N'Sunday Jeremiah Akpan', N'No. 45, Oro Ago Crescent, Garki 2, Abuja', N'08171122334', N'sunday.akpan@gmail.com', 0.00),
(54, N'Ummi Kassim Maiduguri', N'House 30, 2nd Avenue, Gwarinpa, Abuja', N'07068899001', N'ummi.maiduguri@yahoo.com', 0.00),
(55, N'Victor Babatunde Adeleke', N'Plot 77, Ebitu Ukiwe Street, Jabi, Abuja', N'08053344556', N'victor.adeleke@gmail.com', 18500.00),
(56, N'Khadijat Mustapha Bida', N'No. 12, Portharcourt Crescent, Off Gimbiya Street, Garki, Abuja', N'08189900112', N'khadijat.bida@gmail.com', 0.00),
(57, N'Ikechukwu Patrick Obi', N'Shop 2, Area 10 Shopping Centre, Garki, Abuja', N'08031122334', N'ike.obi@gmail.com', 0.00),
(58, N'Zulaiha Ibrahim Zaria', N'House 6, Close 8, Crown Estate, Dawaki, Abuja', N'08104455667', N'zulaiha.zaria@gmail.com', 4300.00),
(59, N'Adekunle Solomon Olatunji', N'Plot 410, Cadastral Zone B03, Wuye, Abuja', N'07035544332', N'kunle.olatunji@gmail.com', 0.00),
(60, N'Binta Salisu Dutse', N'No. 19, Kano Street, Area 1, Garki, Abuja', N'08067788990', N'binta.dutse@yahoo.com', 0.00),

(61, N'Obinna Jude Okereke', N'House 11, 3rd Avenue, Gwarinpa, Abuja', N'08131122334', N'obinna.okereke@gmail.com', 12000.00),
(62, N'Amina Sani Zangon-Daura', N'Plot 120, Panama Street, Maitama, Abuja', N'08098877665', N'amina.zangon@gmail.com', 0.00),
(63, N'Kayode Emmanuel Ojo', N'Suite 401, Metro Plaza, Central Business District, Abuja', N'08123344556', N'kayode.ojo@metro-law.ng', 75000.00),
(64, N'Maryam Mukhtar Ramalan', N'House 16, Road 2, EFAB Estate, Life Camp, Abuja', N'07081122334', N'maryam.ramalan@gmail.com', 0.00),
(65, N'Chidi Stanley Nwosu', N'No. 33, Ogbomosho Street, Area 8, Garki, Abuja', N'08036655443', N'chidi.nwosu@gmail.com', 0.00),
(66, N'Fatima Ahmed Lokoja', N'Plot 50, Lake Chad Crescent, Maitama, Abuja', N'08142233445', N'fatima.lokoja@gmail.com', 8900.00),
(67, N'Babajide Olawale Shonibare', N'House 5, Close 14, Sun City, Galadimawa, Abuja', N'08024455667', N'jide.shonibare@gmail.com', 0.00),
(68, N'Saadatu Umar Sokoto', N'No. 18, Jos Street, Area 3, Garki, Abuja', N'08179900112', N'saadatu.sokoto@yahoo.com', 0.00),
(69, N'Kenechukwu Anthony Nnamani', N'Shop 8, Utako Market Extension, Utako, Abuja', N'07064433221', N'kene.nnamani@gmail.com', 3400.00),
(70, N'Zahra Aliyu Kebbi', N'House 40, 1st Avenue, Model Estate, Lugbe, Abuja', N'08052233445', N'zahra.kebbi@gmail.com', 0.00),

(71, N'Usman Bello Keffi', N'Plot 92, Cadastral Zone C02, Lokogoma, Abuja', N'08181122334', N'usman.keffi@gmail.com', 0.00),
(72, N'Chiamaka Jennifer Ekeh', N'Flat 12, Block 4, FHA Estate, Karu, Abuja', N'08038899001', N'chiamaka.ekeh@gmail.com', 2100.00),
(73, N'Dahiru Mohammed Bauchi', N'House 14, Road 8, Games Village, Kaura, Abuja', N'08107788990', N'dahiru.bauchi@gmail.com', 0.00),
(74, N'Oluwatobi Israel Alabi', N'Plot 201, Maputo Street, Zone 3, Wuse, Abuja', N'07032211009', N'tobi.alabi@gmail.com', 0.00),
(75, N'Hafsat Garba Suleiman', N'No. 7, Gana Street, Maitama, Abuja', N'08061122334', N'hafsat.suleiman@gmail.com', 95000.00),
(76, N'Chinedu Francis Egwuonwu', N'Suite 19, Wuse Shopping Complex, Wuse 1, Abuja', N'08138877665', N'chinedu.egwuonwu@gmail.com', 0.00),
(77, N'Aisha Lawal Katsina', N'House 29, 5th Avenue, Gwarinpa, Abuja', N'08094433221', N'aisha.katsina@gmail.com', 0.00),
(78, N'Tewogbade Sunday Ogundele', N'Plot 33, Boundary Road, Mpape, Abuja', N'08126655443', N'tewo.ogundele@gmail.com', 1100.00),
(79, N'Ruqayyah Abdullahi Sule', N'House 8, Close 4, Crown Estate, Dawaki, Abuja', N'07083344556', N'ruqayyah.sule@gmail.com', 0.00),
(80, N'Kelechi Augustine Nwachukwu', N'Shop 14, Area 11 Market, Garki, Abuja', N'08037766554', N'kelechi.nwachukwu@gmail.com', 0.00),

(81, N'Sanusi Abubakar Lafia', N'Plot 115, Yakubu Gowon Way, Asokoro, Abuja', N'08141122334', N'sanusi.lafia@gmail.com', 42000.00),
(82, N'Nneka Maureen Okoye', N'House 3, Road 15, Federal Housing, Lugbe, Abuja', N'08029988776', N'nneka.okoye@gmail.com', 0.00),
(83, N'Abba Kyari Maiduguri', N'No. 22, Ibrahim Babangida Way, Maitama, Abuja', N'08174455667', N'abba.kyari@gmail.com', 0.00),
(84, N'Yetunde Olayinka Balogun', N'Suite 203, Ceddi Plaza, Central Business District, Abuja', N'07069900112', N'yetunde.balogun@gmail.com', 16500.00),
(85, N'Mustapha Ali Yola', N'House 12, 4th Avenue, Model City, Kubwa, Abuja', N'08057788990', N'mustapha.yola@gmail.com', 0.00),
(86, N'Ifunanya Cynthia Ezekwesili', N'Plot 88, Cadastral Zone B09, Kado, Abuja', N'08183344556', N'ifunanya.ezekwesili@gmail.com', 3800.00),
(87, N'Bello Mohammed Birnin-Kebbi', N'No. 14, Usuman Street, Maitama, Abuja', N'08032233445', N'bello.kebbi@gmail.com', 0.00),
(88, N'Olamide Samuel Folarin', N'House 18, Road 2, River Park Estate, Lugbe, Abuja', N'08109900112', N'olamide.folarin@gmail.com', 0.00),
(89, N'Zulfa Ibrahim Minna', N'Plot 40, Awolowo Way, Utako, Abuja', N'07034455667', N'zulfa.minna@gmail.com', 7200.00),
(90, N'Chidozie Patrick Okpara', N'Suite 10, Mabushi Ultramodern Market, Mabushi, Abuja', N'08062211009', N'chidozie.okpara@gmail.com', 0.00);

SET IDENTITY_INSERT Customers OFF;
GO

-- 3. Insert Authentic AMAC Utility & Tariff Bills in Naira (₦)
SET IDENTITY_INSERT Bills ON;

INSERT INTO Bills (BillID, CustomerID, AmountDue, DueDate, IsPaid, Amount, Status) VALUES
(1, 1, 0.00, '2026-06-15', 1, 18500.00, 'Paid'),
(2, 1, 24500.00, '2026-08-15', 0, 24500.00, 'Unpaid'),
(3, 2, 0.00, '2026-05-30', 1, 15000.00, 'Paid'),
(4, 2, 12000.00, '2026-07-30', 0, 12000.00, 'Unpaid'),
(5, 3, 0.00, '2026-06-01', 1, 35000.00, 'Paid'),
(6, 3, 0.00, '2026-07-01', 1, 27500.00, 'Paid'),
(7, 4, 45000.00, '2026-07-28', 0, 45000.00, 'Unpaid'),
(8, 5, 0.00, '2026-06-20', 1, 16000.00, 'Paid'),
(9, 5, 18500.00, '2026-08-10', 0, 18500.00, 'Unpaid'),
(10, 6, 65000.00, '2026-07-25', 0, 65000.00, 'Unpaid'),

(11, 7, 9500.00, '2026-07-31', 0, 9500.00, 'Unpaid'),
(12, 8, 0.00, '2026-06-10', 1, 22000.00, 'Paid'),
(13, 9, 85000.00, '2026-08-30', 0, 85000.00, 'Unpaid'),
(14, 10, 8000.00, '2026-07-29', 0, 8000.00, 'Unpaid'),
(15, 11, 0.00, '2026-06-05', 1, 120000.00, 'Paid'),
(16, 12, 14500.00, '2026-08-01', 0, 14500.00, 'Unpaid'),
(17, 13, 38000.00, '2026-07-27', 0, 38000.00, 'Unpaid'),
(18, 14, 0.00, '2026-06-18', 1, 28000.00, 'Paid'),
(19, 15, 95000.00, '2026-08-20', 0, 95000.00, 'Unpaid'),
(20, 16, 0.00, '2026-06-25', 1, 19500.00, 'Paid'),

(21, 17, 11000.00, '2026-08-05', 0, 11000.00, 'Unpaid'),
(22, 18, 0.00, '2026-06-12', 1, 150000.00, 'Paid'),
(23, 19, 42000.00, '2026-07-29', 0, 42000.00, 'Unpaid'),
(24, 20, 16500.00, '2026-07-31', 0, 16500.00, 'Unpaid'),
(25, 21, 0.00, '2026-06-14', 1, 13500.00, 'Paid'),
(26, 22, 115000.00, '2026-08-25', 0, 115000.00, 'Unpaid'),
(27, 23, 0.00, '2026-06-30', 1, 21000.00, 'Paid'),
(28, 24, 26000.00, '2026-08-02', 0, 26000.00, 'Unpaid'),
(29, 25, 8500.00, '2026-07-26', 0, 8500.00, 'Unpaid'),
(30, 26, 0.00, '2026-06-01', 1, 250000.00, 'Paid'),

(31, 27, 10500.00, '2026-07-30', 0, 10500.00, 'Unpaid'),
(32, 28, 0.00, '2026-06-10', 1, 135000.00, 'Paid'),
(33, 29, 0.00, '2026-06-22', 1, 31000.00, 'Paid'),
(34, 30, 17500.00, '2026-08-04', 0, 17500.00, 'Unpaid'),
(35, 33, 0.00, '2026-06-15', 1, 180000.00, 'Paid'),
(36, 37, 0.00, '2026-06-11', 1, 65000.00, 'Paid'),
(37, 43, 0.00, '2026-06-08', 1, 48000.00, 'Paid'),
(38, 55, 0.00, '2026-06-17', 1, 32000.00, 'Paid'),
(39, 63, 0.00, '2026-06-02', 1, 140000.00, 'Paid'),
(40, 75, 0.00, '2026-06-09', 1, 210000.00, 'Paid'),
(41, 81, 0.00, '2026-06-13', 1, 110000.00, 'Paid'),
(42, 84, 0.00, '2026-06-19', 1, 55000.00, 'Paid');

SET IDENTITY_INSERT Bills OFF;
GO

-- 4. Insert Payment Records in Naira (₦)
SET IDENTITY_INSERT Payments ON;

INSERT INTO Payments (PaymentID, BillID, AmountPaid, PaymentDate, PaymentMethod) VALUES
(1, 1, 18500.00, '2026-06-12 10:14:22', 'Bank Transfer (GTBank)'),
(2, 3, 15000.00, '2026-05-28 14:30:05', 'POS Terminal'),
(3, 5, 35000.00, '2026-05-31 09:45:12', 'Remita Web Pay'),
(4, 6, 40000.00, '2026-07-01 16:20:00', 'Bank Transfer (Access Bank)'),
(5, 8, 16000.00, '2026-06-18 11:05:44', 'USSD (*737#)'),
(6, 12, 22000.00, '2026-06-08 15:10:30', 'Bank Transfer (Zenith Bank)'),
(7, 15, 120000.00, '2026-06-04 09:12:00', 'Bank Transfer (FirstBank)'),
(8, 18, 28000.00, '2026-06-16 13:40:15', 'POS Terminal'),
(9, 20, 19500.00, '2026-06-24 11:25:30', 'USSD (*919#)'),
(10, 22, 195000.00, '2026-06-10 16:50:00', 'Bank Transfer (UBA)'),
(11, 25, 13500.00, '2026-06-13 10:05:10', 'Remita Web Pay'),
(12, 27, 21000.00, '2026-06-28 14:15:20', 'Bank Transfer (Kuda)'),
(13, 30, 250000.00, '2026-05-30 08:30:00', 'Bank Transfer (Zenith Bank)'),
(14, 32, 135000.00, '2026-06-09 12:10:45', 'Remita Web Pay'),
(15, 33, 35100.00, '2026-06-20 15:45:00', 'Bank Transfer (GTBank)'),
(16, 35, 235000.00, '2026-06-14 11:00:00', 'Bank Transfer (Access Bank)'),
(17, 36, 65000.00, '2026-06-10 10:30:00', 'POS Terminal'),
(18, 37, 48000.00, '2026-06-07 14:20:00', 'Remita Web Pay'),
(19, 38, 32000.00, '2026-06-16 09:15:00', 'Bank Transfer (FirstBank)'),
(20, 39, 215000.00, '2026-06-01 17:10:00', 'Bank Transfer (Zenith Bank)'),
(21, 40, 210000.00, '2026-06-08 13:00:00', 'Remita Web Pay'),
(22, 41, 152000.00, '2026-06-12 10:40:00', 'Bank Transfer (GTBank)'),
(23, 42, 71500.00, '2026-06-18 16:05:00', 'POS Terminal');

SET IDENTITY_INSERT Payments OFF;
GO

PRINT '90 Clean Authentic AMAC Users and Naira (₦) Billing Records Loaded Successfully!';
GO
