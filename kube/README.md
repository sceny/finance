# https://v1-15.docs.kubernetes.io/docs/tasks/run-application/run-single-instance-stateful-application/

# https://hub.docker.com/_/postgres

# https://kubernetes.io/blog/2018/04/13/local-persistent-volumes-beta/

# https://severalnines.com/database-blog/using-kubernetes-deploy-postgresql
kubectl apply -f postgres-configmap.yaml
kubectl apply -f postgres-storage.yaml
kubectl apply -f postgres-deployment.yaml
kubectl apply -f postgres-service.yaml

kubectl apply -f postgres-configmap.yaml -f postgres-storage.yaml -f postgres-deployment.yaml -f postgres-service.yaml

kubectl delete -f postgres-configmap.yaml -f postgres-storage.yaml -f postgres-deployment.yaml -f postgres-service.yaml
