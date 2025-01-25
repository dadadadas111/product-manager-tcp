-- Insert Categories
INSERT INTO Categories (id, Name) VALUES 
(NEWID(), 'Electronics'),
(NEWID(), 'Clothing'),
(NEWID(), 'Home Appliances'),
(NEWID(), 'Books'),
(NEWID(), 'Sports');

-- Insert Products
INSERT INTO Products (id, Name, Price, Stock, CategoryId) VALUES
(NEWID(), 'Smartphone', 799.99, 50, (SELECT id FROM Categories WHERE Name = 'Electronics')),
(NEWID(), 'Laptop', 999.99, 30, (SELECT id FROM Categories WHERE Name = 'Electronics')),
(NEWID(), 'Headphones', 149.99, 100, (SELECT id FROM Categories WHERE Name = 'Electronics')),
(NEWID(), 'Tablet', 299.99, 40, (SELECT id FROM Categories WHERE Name = 'Electronics')),
(NEWID(), 'Smartwatch', 199.99, 60, (SELECT id FROM Categories WHERE Name = 'Electronics')),

(NEWID(), 'T-Shirt', 19.99, 200, (SELECT id FROM Categories WHERE Name = 'Clothing')),
(NEWID(), 'Jeans', 39.99, 150, (SELECT id FROM Categories WHERE Name = 'Clothing')),
(NEWID(), 'Jacket', 89.99, 100, (SELECT id FROM Categories WHERE Name = 'Clothing')),
(NEWID(), 'Sweater', 29.99, 180, (SELECT id FROM Categories WHERE Name = 'Clothing')),
(NEWID(), 'Sneakers', 59.99, 120, (SELECT id FROM Categories WHERE Name = 'Clothing')),

(NEWID(), 'Microwave', 119.99, 80, (SELECT id FROM Categories WHERE Name = 'Home Appliances')),
(NEWID(), 'Refrigerator', 599.99, 50, (SELECT id FROM Categories WHERE Name = 'Home Appliances')),
(NEWID(), 'Blender', 49.99, 150, (SELECT id FROM Categories WHERE Name = 'Home Appliances')),
(NEWID(), 'Washing Machine', 399.99, 60, (SELECT id FROM Categories WHERE Name = 'Home Appliances')),
(NEWID(), 'Air Conditioner', 799.99, 30, (SELECT id FROM Categories WHERE Name = 'Home Appliances')),

(NEWID(), 'Novel', 9.99, 300, (SELECT id FROM Categories WHERE Name = 'Books')),
(NEWID(), 'Science Book', 19.99, 200, (SELECT id FROM Categories WHERE Name = 'Books')),
(NEWID(), 'Biography', 14.99, 150, (SELECT id FROM Categories WHERE Name = 'Books')),
(NEWID(), 'Comic Book', 5.99, 250, (SELECT id FROM Categories WHERE Name = 'Books')),
(NEWID(), 'Cookbook', 24.99, 100, (SELECT id FROM Categories WHERE Name = 'Books')),

(NEWID(), 'Basketball', 29.99, 180, (SELECT id FROM Categories WHERE Name = 'Sports')),
(NEWID(), 'Soccer Ball', 19.99, 200, (SELECT id FROM Categories WHERE Name = 'Sports')),
(NEWID(), 'Tennis Racket', 49.99, 100, (SELECT id FROM Categories WHERE Name = 'Sports')),
(NEWID(), 'Baseball Glove', 39.99, 150, (SELECT id FROM Categories WHERE Name = 'Sports')),
(NEWID(), 'Golf Club Set', 299.99, 50, (SELECT id FROM Categories WHERE Name = 'Sports'));
