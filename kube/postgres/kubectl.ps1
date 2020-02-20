
# https://severalnines.com/database-blog/using-kubernetes-deploy-postgresql
kubectl apply -f postgres-configmap.yaml
kubectl apply -f postgres-storage.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f postgres-service.yaml

kubectl apply `
    -f postgres-configmap.yaml `
    -f postgres-storage.yaml `
    -f postgres-deployment.yaml `
    -f postgres-service.yaml

kubectl delete `
    -f postgres-configmap.yaml `
    -f postgres-storage.yaml `
    -f postgres-deployment.yaml `
    -f postgres-service.yaml


kubectl get service postgres