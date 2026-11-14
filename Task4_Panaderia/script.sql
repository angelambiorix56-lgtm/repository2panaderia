-- Angel Liriano
-- Proyecto: Registro de Productos para Panadería - TAREA 4
-- Script de creación de la base de datos (SQL Server)


IF DB_ID('PanaderiaDB') IS NULL
    CREATE DATABASE PanaderiaDB;
GO

USE PanaderiaDB;
GO

IF OBJECT_ID('Categorias') IS NULL
    CREATE TABLE Categorias
    (
        Id     INT IDENTITY(1,1) PRIMARY KEY,
        Nombre NVARCHAR(50) NOT NULL UNIQUE
    );
GO

IF OBJECT_ID('Productos') IS NULL
    CREATE TABLE Productos
    (
        Codigo      INT IDENTITY(1,1) PRIMARY KEY,
        Nombre      NVARCHAR(100) NOT NULL UNIQUE,
        CategoriaId INT NOT NULL FOREIGN KEY REFERENCES Categorias(Id),
        Precio      DECIMAL(10,2) NOT NULL CHECK (Precio > 0)
    );
GO

-- Datos iniciales (solo si las tablas están vacías)
IF NOT EXISTS (SELECT 1 FROM Categorias)
BEGIN
    INSERT INTO Categorias (Nombre) VALUES ('Panes'), ('Bizcochos'), ('Galletas'), ('Bebidas');

    INSERT INTO Productos (Nombre, CategoriaId, Precio)
    SELECT 'Pan de agua', Id, 10 FROM Categorias WHERE Nombre = 'Panes';
    INSERT INTO Productos (Nombre, CategoriaId, Precio)
    SELECT 'Pan sobao', Id, 15 FROM Categorias WHERE Nombre = 'Panes';
    INSERT INTO Productos (Nombre, CategoriaId, Precio)
    SELECT 'Bizcocho dominicano (libra)', Id, 450 FROM Categorias WHERE Nombre = 'Bizcochos';
    INSERT INTO Productos (Nombre, CategoriaId, Precio)
    SELECT 'Galletas de mantequilla', Id, 75 FROM Categorias WHERE Nombre = 'Galletas';
    INSERT INTO Productos (Nombre, CategoriaId, Precio)
    SELECT 'Jugo de chinola', Id, 60 FROM Categorias WHERE Nombre = 'Bebidas';
END
GO
