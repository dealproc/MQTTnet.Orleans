#!/bin/bash
set -e
# echo "processing params"
while [ "$1" != "" ]; do
    case $1 in
        -n | --namespace)
            NAMESPACE=$2
            shift 2
            ;;
        -s | --selector)
            SELECTOR=$2
            shift 2
            ;;
        *)
            PARAMS="$PARAMS$1 "
            shift 1
            ;;
    esac
done
# echo "serching for pod in ${NAMESPACE:-default} namespace and with $SELECTOR"
POD=`kubectl get pods -n ${NAMESPACE:-default} --selector=$SELECTOR -o jsonpath='{.items[0].metadata.name}'`;
# echo "starting debugger on $POD";
kubectl exec $POD -i -- $PARAMS;