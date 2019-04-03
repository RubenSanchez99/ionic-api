INSERT INTO "CatalogType" ("Type") VALUES 
('Mug')
,('T-Shirt')
,('Sheet')
,('USB Memory Stick')
;

INSERT INTO "CatalogBrand" ("Brand") VALUES 
('Azure')
,('.NET')
,('Visual Studio')
,('SQL Server')
,('Other')
;

INSERT INTO "Catalog" ("Name","Description","Price","PictureFileName","CatalogTypeId","CatalogBrandId","AvailableStock","RestockThreshold","MaxStockThreshold","OnReorder") VALUES 
('.NET Bot Black Hoodie','.NET Bot Black Hoodie, and more',19.50,'1.png',2,2,100,0,200,False)
,('.NET Black & White Mug','.NET Black & White Mug',8.50,'2.png',1,3,89,0,200,True)
,('Prism White T-Shirt','Prism White T-Shirt',12.00,'3.png',2,5,56,0,200,False)
,('.NET Foundation T-shirt','.NET Foundation T-shirt',12.00,'4.png',2,2,120,0,200,False)
,('Roslyn Red Sheet','Roslyn Red Sheet',8.50,'5.png',3,5,55,0,200,False)
,('.NET Blue Hoodie','.NET Blue Hoodie',12.00,'6.png',2,2,17,0,200,False)
,('Roslyn Red T-Shirt','Roslyn Red T-Shirt',12.00,'7.png',2,5,8,0,200,False)
,('Kudu Purple Hoodie','Kudu Purple Hoodie',8.50,'8.png',2,5,34,0,200,False)
,('Cup<T> White Mug','Cup<T> White Mug',12.00,'9.png',1,5,76,0,200,False)
,('.NET Foundation Sheet','.NET Foundation Sheet',12.00,'10.png',3,2,11,0,200,False)
,('Cup<T> Sheet','Cup<T> Sheet',8.50,'11.png',3,2,3,0,200,False)
,('Prism White TShirt','Prism White TShirt',12.00,'12.png',2,5,0,0,200,False)
;