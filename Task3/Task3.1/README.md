# Jaeger в Minikube с сервисами

## Описание
Развертывание Jaeger в Minikube с двумя сервисами, которые:
1. Взаимодействуют между собой
2. Отправляют трейсы в Jaeger

## Требования
- Minikube
- kubectl
- Docker

## Установка

### 1. Запуск Minikube 
```bash
minikube start --addons=ingress 
```
Ingress нужен для вызовов

### 2. Установка cert-manager
```bash
kubectl apply -f https://github.com/cert-manager/cert-manager/releases/download/v1.13.3/cert-manager.yaml
```

### 3. Развертывание Jaeger
```bash
kubectl create namespace observability
kubectl create -f ./k8s/jaeger-operator.yaml -n observability

# Подождите, пока оператор полностью запустится
kubectl wait --for=condition=ready pod -l name=jaeger-operator -n observability --timeout=60s

kubectl apply -f k8s/jaeger-instance.yaml
```

### 4. Сборка и деплой сервисов
```bash
# Сборка образов
minikube image build -t service-a:latest services/service-a/
minikube image build -t service-b:latest services/service-b/

# Развертывание
kubectl apply -f k8s/services.yaml

# Удаление
kubectl delete pod -l app=service-a
kubectl delete pod -l app=service-b
```

## Проверка работы

### Доступ к Jaeger UI
```bash
kubectl port-forward services/service-a 8080:8080
kubectl port-forward services/service-b 8081:8080
kubectl port-forward svc/simplest-query 16686:16686
```
Откройте в браузере: http://localhost:16686

### Тестирование сервисов
```bash
# Вызов service-a, который вызывает service-b
kubectl exec -it $(kubectl get pods -l app=service-a -o jsonpath='{.items[0].metadata.name}') -- wget -qO- http://service-a:8080
```

## Структура проекта
- `services/service-a/` - Исходный код service-a
- `services/service-b/` - Исходный код service-b  
- `k8s/services.yaml` - Конфигурация Kubernetes для сервисов
- `jaeger-instance.yaml` - Конфигурация Jaeger