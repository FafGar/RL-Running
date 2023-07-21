using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NeuralNet
{
    double [] inputWeights;
    double [,] layer1Weights;
    double [,]layer2Weights;
    double [] actionWeights;

    int numInputs;
    int numOutputOptions;

    void init(int numInputs, int numOutputOptions){
        this.numInputs = numInputs;
        this.numOutputOptions = numOutputOptions;

        inputWeights = new double[numInputs];
        layer1Weights = new double[numInputs*4,numOutputOptions*4];
        layer2Weights = new double[numOutputOptions*4, numOutputOptions];
        actionWeights = new double[numOutputOptions];

        for(int i=0;i<numInputs*4;i++){
            if(i%4 == 0){
                inputWeights[i/4] = Random.Range(1,50)/10d;
            }
            for(int j = 0; j<numOutputOptions*4;j++){
                layer1Weights[i,j] = Random.Range(1,50)/10d;
            }
        }

        for(int i=0;i<numOutputOptions*4;i++){
            if(i%4 == 0){
                actionWeights[i/4] = Random.Range(1,50)/10d;
            }
            for(int j = 0; j<numOutputOptions;j++){
                layer1Weights[i,j] = Random.Range(1,50)/10d;
            }
        }
    }

    int evaluate(float[] inputs){
        if(inputs.Length != numInputs){
            return -1;
        }

        double[] input = new double[this.numInputs];
        double[,] layer1 = new double[this.numInputs*4,this.numOutputOptions*4];
        double[,] layer2 = new double[this.numOutputOptions*4, this.numOutputOptions];
        double[] action = new double[this.numOutputOptions];

        for(int i = 0;i<this.numInputs;i++){
            input[i] = inputs[i]*inputWeights[i];
        }
        for(int i = 0;i<this.numInputs*4;i++){
            for(int j=0;j<this.numOutputOptions*4;j++){

            }
        }

        // Go through the rest of the layers, applying weights to input from previous layer
        // return index of max value from action option layer
        return -1;
    }

    void train(int reward){
        //strongest and weakest 20% of the final weights are adjusted by adding or subtracting some multiple of the reward
        //Recurse this process back through the layers until reaching the input layer.
    }
}
