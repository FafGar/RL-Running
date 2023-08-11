using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class NeuralNet
{
    struct neuron
    {
        public double [] weights;
        public double bias;
    }


    neuron [] layer1Neurons;
    neuron [] layer2Neurons;
    neuron [] layer3Neurons;
    neuron [] actionNeurons;

    double[] layer1Vals;
    double[] layer2Vals;
    double[] layer3Vals;
    double[] actionVals;

    int layer1MaxI;
    int layer2MaxI;
    int layer3MaxI;
    int outputLayerMaxI;

    double [] oldInputs;
    int numInputs;
    int numOutputOptions;
    int layerSize;
    int maxReward;
    double trainingCoefficient;
    double discountRate;

    /* TODO : Make the layer evaluation go through each possible action. OR MAYBE DON'T? You may not need to because it is already being estimated. I'M TIRED 
    */ 

    public void init(int numInputs, int outputOptionsCount, int layerSize, double discountRate, double trainingCoefficient){
        this.numInputs = numInputs;
        this.numOutputOptions = outputOptionsCount+1; //adding room for a do nothing option
        this.layerSize = layerSize;
        this.trainingCoefficient = trainingCoefficient;
        this.discountRate = discountRate;

        layer1Neurons = new neuron[layerSize];
        layer2Neurons = new neuron[layerSize];
        layer3Neurons = new neuron[layerSize];
        actionNeurons = new neuron[numOutputOptions];

        //Note for tryin to understand this later: Each layer has a number of neurons. Each neuron has their own set of weights with a # relative to the previous layer's # of neurons.
        for(int i=0;i<layerSize;i++){
            // layer1Neurons[i] = new neuron();
            // layer2Neurons[i] = new neuron();
            // layer3Neurons[i] = new neuron();
            

            layer1Neurons[i].bias = Random.Range(0,50)/10d;
            layer1Neurons[i].weights = new double[numInputs];

            layer2Neurons[i].bias =Random.Range(0,50)/10d;
            layer2Neurons[i].weights = new double[layerSize];

            layer3Neurons[i].bias = Random.Range(0,50)/10d;
            layer3Neurons[i].weights = new double[layerSize];

            for(int j=0; j<numInputs; j++) layer1Neurons[i].weights[j] = Random.Range(1,50)/10d;
            for(int j = 0;j<layerSize;j++){
                layer2Neurons[i].weights[j] = Random.Range(1,50)/10d;
                layer3Neurons[i].weights[j] = Random.Range(1,50)/10d;
            }
            
        }

        for(int i=0;i<numOutputOptions;i++){
            actionNeurons[i] = new neuron
            {
                bias = Random.Range(1, 50) / 10d,
                weights = new double[layerSize]
            };

            for (int j = 0;j<layerSize;j++){
                actionNeurons[i].weights[j] = Random.Range(1,50)/10d;
            }
        }

        this.layer1Vals = new double[this.layerSize];
        this.layer2Vals = new double[this.layerSize];
        this.layer3Vals = new double[this.layerSize];
        this.actionVals = new double[this.numOutputOptions];
    }

    public int evaluate(double[] inputs){
    //Note: inputs[0] must be the integer number of the action taken

        if(inputs.Length != this.numInputs){
            return -1;
        }
        this.oldInputs = new double[inputs.Length];
        System.Array.Copy(inputs, this.oldInputs, inputs.Length);

        this.layer1Vals = new double[this.layerSize];
        this.layer2Vals = new double[this.layerSize];
        this.layer3Vals = new double[this.layerSize];
        this.actionVals = new double[this.numOutputOptions];
       

        this.layer1MaxI = this.evalLayer(ref layer1Vals, layer1Neurons, inputs);

        this.layer2MaxI = this.evalLayer(ref layer2Vals, layer2Neurons, layer1Vals);

        this.layer3MaxI = this.evalLayer(ref layer3Vals, layer3Neurons, layer2Vals);

        this.outputLayerMaxI = this.evalLayer(ref actionVals, actionNeurons, layer3Vals);
        // Debug.Log(actionVals[0]);
        return this.outputLayerMaxI;
    }

    public double getQ(int actionIndex){
        return actionVals[actionIndex];
    }

    public void train(int reward, double [] inputs){
        //Note: inputs[0] must be the integer number of the action taken


        /* MATH TIME!
            All the weight functions are linear in a multivariable space. So their gradients are always just one value per variable.
            The wieght functinos must be differentiated with respect to the weight, however, so the values are really just the input values from the previous layer.
            Here is the generic weight approximation function:

            w^ = w^ + trainingCoefficient(reward + discountRate(maximum predicted reward value in new state) - (Q from this state)*gradient)
            
            This evaluation should move the various weights up or down relative to the reward because it and the weights are capable of being negative
            The same thing will be applied to the bias.
        */

        //Pre-training data
        double actionQ = this.actionVals[outputLayerMaxI];
        double layer3Q = this.layer3Vals[layer3MaxI];
        double layer2Q = this.layer2Vals[layer2MaxI];
        double layer1Q = this.layer1Vals[layer1MaxI];

        //get new Q values
        double [] newQs = this.predictQ(inputs);

        //layer1 training
        double newlayer1Q = newQs[0];
        trainLayer(layer1Neurons, layer1Q, newlayer1Q, reward);

        //layer 2 traning
        double newlayer2Q = newQs[1];
        trainLayer(layer2Neurons, layer2Q, newlayer2Q, reward);

        //layer 3 training
        double newlayer3Q = newQs[2];
        trainLayer(layer3Neurons, layer3Q, newlayer3Q, reward); 

        //Output Layer training
        double newActionQ = newQs[3];
        trainLayer(actionNeurons, actionQ, newActionQ, reward);
        }

    double [] predictQ(double[] inputs)
    {
        double max = 0;
        double [] newQs = new double[4];
        for(int i =0; i<this.numInputs; i++){
            this.evaluate(inputs);
            if(this.actionVals[this.outputLayerMaxI] > max){
                max = this.actionVals[this.outputLayerMaxI];
                newQs[0] = layer1Vals[layer1MaxI];
                newQs[1] = layer2Vals[layer2MaxI];
                newQs[2] = layer3Vals[layer3MaxI];
                newQs[3] = actionVals[outputLayerMaxI];
            }
        }
        
        return newQs;
    }

    int evalLayer(ref double[] outputArr, neuron[] neuronArr, double[] inputArr){
        int max_index = 0;
        for(int i = 0; i< outputArr.Length; i++){
            outputArr[i] = neuronArr[i].bias;
            for(int j = 0; j<inputArr.Length; j++) outputArr[i] += inputArr[j]*neuronArr[i].weights[j];
            if(outputArr[i]>outputArr[max_index]) max_index = i;
        }
        return max_index;
    }
    

    void trainLayer(neuron[] layerNeruons, double oldQ, double newQ, int reward){
        for(int i = 0;i<layerNeruons.Length;i++){
            for(int j = 0; j<this.oldInputs.Length; j++) {
                layerNeruons[i].weights[j] = System.Math.Abs(layerNeruons[i].weights[j]  + 
                                            this.trainingCoefficient*
                                            (reward + this.discountRate*newQ - oldQ*this.oldInputs[j]));
            }
        }
    }
}
