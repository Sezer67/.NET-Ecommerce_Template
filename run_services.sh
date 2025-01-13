#!/bin/bash

# APIGateway'i başlat
dotnet run --project ${workspaceFolder}/src/APIGateway/ECommerce.APIGateway &

# UserService'i başlat
dotnet run --project ${workspaceFolder}/src/UserService/ECommerce.UserService &

# CartService'i başlat
dotnet run --project ${workspaceFolder}/src/CartService/ECommerce.CartService &

# ProductService'i başlat
dotnet run --project ${workspaceFolder}/src/ProductService/ECommerce.ProductService &

# Wait for all services to start
wait